namespace Template.WindowsApp.Services;

using Microsoft.Data.Sqlite;

using Smart.Data;

using Template.WindowsApp.Helpers.Data;
using Template.WindowsApp.Models.Entity;
using Template.WindowsApp.Models.Sample;

public enum ProductSort
{
    Code,
    Name,
    Category,
    Supplier,
    Warehouse,
    Price,
    Stock,
    Updated,
    IsActive
}

public sealed record DatabaseInfo(
    string Path,
    long Size,
    long ProductCount,
    long CustomerCount,
    long OrderLineCount,
    long DataCount,
    long BulkDataCount);

public sealed class DataService
{
    private readonly TimeProvider timeProvider;

    private readonly IDbProvider provider;

    private readonly IDialect dialect;

    private readonly DataAccessor accessor;

    public DataService(
        TimeProvider timeProvider,
        IDbProvider provider,
        IDialect dialect,
        DataAccessor accessor)
    {
        this.timeProvider = timeProvider;
        this.provider = provider;
        this.dialect = dialect;
        this.accessor = accessor;
    }

    public string DatabasePath
    {
        get
        {
            using var con = provider.CreateConnection();
            return Path.GetFullPath(con.DataSource);
        }
    }

    //--------------------------------------------------------------------------------
    // Database
    //--------------------------------------------------------------------------------

    public ValueTask InitializeAsync() =>
        File.Exists(DatabasePath) ? ValueTask.CompletedTask : RebuildAsync();

    public async ValueTask RebuildAsync()
    {
        var path = DatabasePath;

        SqliteConnection.ClearAllPools();
        foreach (var file in new[] { path, $"{path}-wal", $"{path}-shm" })
        {
            if (File.Exists(file))
            {
                File.Delete(file);
            }
        }
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        await provider.UsingAsync(async con =>
        {
            await accessor.ExecutePragmaAsync(con).ConfigureAwait(false);
            await accessor.CreateTablesAsync(con).ConfigureAwait(false);
        }).ConfigureAwait(false);

        var today = timeProvider.GetLocalNow().Date;
        await provider.UsingTxAsync(async (_, tx) =>
        {
            foreach (var entity in ProductSample.LoadProducts())
            {
                await accessor.InsertProductAsync(tx, entity).ConfigureAwait(false);
            }
            foreach (var entity in CustomerSample.LoadCustomers())
            {
                await accessor.InsertCustomerEntityAsync(tx, entity).ConfigureAwait(false);
            }
            foreach (var entity in OrderSample.LoadLines(today))
            {
                await accessor.InsertOrderLineAsync(tx, entity).ConfigureAwait(false);
            }

            await tx.CommitAsync().ConfigureAwait(false);
        }).ConfigureAwait(false);
    }

    public async ValueTask<DatabaseInfo> GetDatabaseInfoAsync()
    {
        var path = DatabasePath;
        var file = new FileInfo(path);
        var wal = new FileInfo($"{path}-wal");
        return new DatabaseInfo(
            path,
            (file.Exists ? file.Length : 0) + (wal.Exists ? wal.Length : 0),
            await accessor.CountProductAsync().ConfigureAwait(false),
            await accessor.CountCustomerAsync().ConfigureAwait(false),
            await accessor.CountOrderLineAsync().ConfigureAwait(false),
            await accessor.CountDataAsync().ConfigureAwait(false),
            await accessor.CountBulkDataAsync().ConfigureAwait(false));
    }

    //--------------------------------------------------------------------------------
    // Product
    //--------------------------------------------------------------------------------

    public long CountProducts(string keyword) =>
        accessor.CountProductMatch(dialect.Match(keyword));

    public IReadOnlyList<ProductEntity> QueryProducts(string keyword, ProductSort sort, bool desc, int offset, int limit) =>
        accessor.QueryProductList(dialect.Match(keyword), sort, desc, limit, offset);

    public ValueTask<int> UpdateProductAsync(ProductEntity entity) =>
        accessor.UpdateProductAsync(entity);

    public ValueTask ResetProductsAsync() =>
        provider.UsingTxAsync(async (_, tx) =>
        {
            await accessor.DeleteAllProductAsync(tx).ConfigureAwait(false);
            foreach (var entity in ProductSample.LoadProducts())
            {
                await accessor.InsertProductAsync(tx, entity).ConfigureAwait(false);
            }

            await tx.CommitAsync().ConfigureAwait(false);
        });

    //--------------------------------------------------------------------------------
    // Customer
    //--------------------------------------------------------------------------------

    public IReadOnlyList<CustomerEntity> QueryCustomers() =>
        accessor.QueryCustomerList();

    public async ValueTask<CustomerEntity> InsertCustomerAsync(string name)
    {
        var entity = new CustomerEntity
        {
            Name = name,
            Company = string.Empty,
            Email = string.Empty,
            Phone = string.Empty,
            City = string.Empty
        };
        entity.Id = await accessor.InsertCustomerAsync(entity.Name, entity.Company, entity.Email, entity.Phone, entity.City).ConfigureAwait(false);
        return entity;
    }

    public ValueTask<int> UpdateCustomerAsync(CustomerEntity entity) =>
        accessor.UpdateCustomerAsync(entity);

    public ValueTask<int> DeleteCustomerAsync(long id) =>
        accessor.DeleteCustomerAsync(id);

    //--------------------------------------------------------------------------------
    // OrderLine
    //--------------------------------------------------------------------------------

    public IReadOnlyList<OrderLineEntity> QueryOrderLines() =>
        accessor.QueryOrderLineList();

    public ValueTask ReplaceOrderLinesAsync(IEnumerable<OrderLineEntity> source) =>
        provider.UsingTxAsync(async (_, tx) =>
        {
            await accessor.DeleteAllOrderLineAsync(tx).ConfigureAwait(false);
            foreach (var entity in source)
            {
                await accessor.InsertOrderLineAsync(tx, entity).ConfigureAwait(false);
            }

            await tx.CommitAsync().ConfigureAwait(false);
        });

    //--------------------------------------------------------------------------------
    // Data
    //--------------------------------------------------------------------------------

    public async ValueTask<bool> InsertDataAsync(DataEntity entity)
    {
        try
        {
            await accessor.InsertDataAsync(entity).ConfigureAwait(false);
            return true;
        }
        catch (DbException e) when (dialect.IsDuplicate(e))
        {
            return false;
        }
    }

    public ValueTask<int> UpdateDataAsync(long id, string name) =>
        accessor.UpdateDataAsync(id, name);

    public ValueTask<int> DeleteDataAsync(long id) =>
        accessor.DeleteDataAsync(id);

    public ValueTask<DataEntity?> QueryDataAsync(long id) =>
        accessor.QueryDataAsync(id);

    //--------------------------------------------------------------------------------
    // BulkData
    //--------------------------------------------------------------------------------

    public ValueTask<long> CountBulkDataAsync() =>
        accessor.CountBulkDataAsync();

    public void InsertBulkData(IEnumerable<BulkDataEntity> source) =>
        provider.UsingTx((_, tx) =>
        {
            foreach (var entity in source)
            {
                accessor.InsertBulkData(tx, entity);
            }

            tx.Commit();
        });

    public ValueTask<int> DeleteAllBulkDataAsync() =>
        accessor.DeleteAllBulkDataAsync();

    public IReadOnlyList<BulkDataEntity> QueryAllBulkData() =>
        accessor.QueryAllBulkDataList();
}
