```sql

-- Database: pgvectors

-- DROP DATABASE IF EXISTS pgvectors;

CREATE DATABASE pgvectors
    WITH
    OWNER = sa
    ENCODING = 'UTF8'
    LC_COLLATE = 'en_US.utf8'
    LC_CTYPE = 'en_US.utf8'
    TABLESPACE = pg_default
    CONNECTION LIMIT = -1
    IS_TEMPLATE = False;



-- Table: public.embeddings

-- DROP TABLE IF EXISTS public.embeddings;

CREATE TABLE IF NOT EXISTS public.embeddings
(
    id bigserial,
    content character varying(1024) COLLATE pg_catalog."default",
    vector_id integer,
    embedding vector(1536)
)

TABLESPACE pg_default;



ALTER TABLE IF EXISTS public.embeddings
    OWNER to sa;



-- Index: ivfflat_embedding

-- DROP INDEX IF EXISTS public.ivfflat_embedding;

CREATE INDEX IF NOT EXISTS ivfflat_embedding
    ON public.embeddings USING ivfflat
    (embedding)
    TABLESPACE pg_default;

```