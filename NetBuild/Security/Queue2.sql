CREATE SCHEMA [Queue2]
    AUTHORIZATION [dbo];


GO
GRANT SELECT
    ON SCHEMA::[Queue2] TO [BuildQueue];


GO
GRANT INSERT
    ON SCHEMA::[Queue2] TO [BuildQueue];


GO
GRANT DELETE
    ON SCHEMA::[Queue2] TO [BuildQueue];

