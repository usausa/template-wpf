SELECT
    *
FROM
    Product
/*% if (keyword != null) { */
WHERE
    Code LIKE /*@ keyword */'' ESCAPE '\'
    OR Name LIKE /*@ keyword */'' ESCAPE '\'
/*% } */
ORDER BY
/*% if (desc) { */
    /*# sort */Code DESC,
/*% } else { */
    /*# sort */Code,
/*% } */
    Code
LIMIT /*@ limit */15 OFFSET /*@ offset */0
