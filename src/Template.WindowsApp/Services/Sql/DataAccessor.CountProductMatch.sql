SELECT
    COUNT(*)
FROM
    Product
/*% if (keyword is not null) { */
WHERE
    Code LIKE /*@ keyword */'' ESCAPE '\'
    OR Name LIKE /*@ keyword */'' ESCAPE '\'
/*% } */
