SELECT
    COUNT(*)
FROM
    Product
/*% if (keyword != null) { */
WHERE
    Code LIKE /*@ keyword */'' ESCAPE '\'
    OR Name LIKE /*@ keyword */'' ESCAPE '\'
/*% } */
