CREATE SCHEMA [Queue]
    AUTHORIZATION [dbo];


GO
GRANT UPDATE
    ON SCHEMA::[Queue] TO [BuildQueue];


GO
GRANT SELECT
    ON SCHEMA::[Queue] TO [BuildQueue];


GO
GRANT INSERT
    ON SCHEMA::[Queue] TO [BuildQueue];


GO
GRANT EXECUTE
    ON SCHEMA::[Queue] TO [BuildQueue];


GO
GRANT DELETE
    ON SCHEMA::[Queue] TO [BuildQueue];

