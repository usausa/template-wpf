namespace Template.WindowsApp.Services;

using Smart.Data.Accessor.Attributes;

using Template.WindowsApp.Models;
using Template.WindowsApp.Models.Entity;

[DataAccessor]
public sealed partial class DataAccessor
{
    //--------------------------------------------------------------------------------
    // Initialize
    //--------------------------------------------------------------------------------

    [Execute]
    public partial ValueTask<int> ExecutePragmaAsync(DbConnection con);

    [Execute]
    public partial ValueTask<int> CreateTablesAsync(DbConnection con);

    //--------------------------------------------------------------------------------
    // Product
    //--------------------------------------------------------------------------------

    [ExecuteScalar]
    [Count(typeof(ProductEntity))]
    public partial ValueTask<long> CountProductAsync();

    [ExecuteScalar]
    public partial long CountProductMatch(string? keyword);

    [Query]
    public partial IReadOnlyList<ProductEntity> QueryProductList(string? keyword, ProductSort sort, bool desc, int limit, int offset);

    [Execute]
    [Insert(typeof(ProductEntity))]
    public partial ValueTask<int> InsertProductAsync(DbTransaction tx, ProductEntity entity);

    [Execute]
    [Update(typeof(ProductEntity))]
    public partial ValueTask<int> UpdateProductAsync(ProductEntity entity);

    [Execute]
    [Delete(typeof(ProductEntity))]
    public partial ValueTask<int> DeleteAllProductAsync(DbTransaction tx);

    //--------------------------------------------------------------------------------
    // Customer
    //--------------------------------------------------------------------------------

    [ExecuteScalar]
    [Count(typeof(CustomerEntity))]
    public partial ValueTask<long> CountCustomerAsync();

    [Query]
    public partial IReadOnlyList<CustomerEntity> QueryCustomerList();

    [ExecuteScalar]
    public partial ValueTask<long> InsertCustomerAsync(string name, string company, string email, string phone, string city);

    [Execute]
    [Insert(typeof(CustomerEntity))]
    public partial ValueTask<int> InsertCustomerEntityAsync(DbTransaction tx, CustomerEntity entity);

    [Execute]
    [Update(typeof(CustomerEntity))]
    public partial ValueTask<int> UpdateCustomerAsync(CustomerEntity entity);

    [Execute]
    [Delete(typeof(CustomerEntity))]
    public partial ValueTask<int> DeleteCustomerAsync(long id);

    //--------------------------------------------------------------------------------
    // OrderLine
    //--------------------------------------------------------------------------------

    [ExecuteScalar]
    [Count(typeof(OrderLineEntity))]
    public partial ValueTask<long> CountOrderLineAsync();

    [Query]
    public partial IReadOnlyList<OrderLineEntity> QueryOrderLineList();

    [Execute]
    [Insert(typeof(OrderLineEntity))]
    public partial ValueTask<int> InsertOrderLineAsync(DbTransaction tx, OrderLineEntity entity);

    [Execute]
    [Delete(typeof(OrderLineEntity))]
    public partial ValueTask<int> DeleteAllOrderLineAsync(DbTransaction tx);

    //--------------------------------------------------------------------------------
    // Data
    //--------------------------------------------------------------------------------

    [ExecuteScalar]
    [Count(typeof(DataEntity))]
    public partial ValueTask<long> CountDataAsync();

    [Execute]
    [Insert(typeof(DataEntity))]
    public partial ValueTask<int> InsertDataAsync(DataEntity entity);

    [Execute]
    public partial ValueTask<int> UpdateDataAsync(long id, string name);

    [Execute]
    [Delete(typeof(DataEntity))]
    public partial ValueTask<int> DeleteDataAsync(long id);

    [QueryFirst]
    [SelectSingle(typeof(DataEntity))]
    public partial ValueTask<DataEntity?> QueryDataAsync(long id);

    //--------------------------------------------------------------------------------
    // BulkData
    //--------------------------------------------------------------------------------

    [ExecuteScalar]
    [Count(typeof(BulkDataEntity))]
    public partial ValueTask<long> CountBulkDataAsync();

    [Execute]
    [Insert(typeof(BulkDataEntity))]
    public partial int InsertBulkData(DbTransaction tx, BulkDataEntity entity);

    [Execute]
    [Delete(typeof(BulkDataEntity))]
    public partial ValueTask<int> DeleteAllBulkDataAsync();

    [Query]
    public partial IReadOnlyList<BulkDataEntity> QueryAllBulkDataList();
}
