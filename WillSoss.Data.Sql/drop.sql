alter database [{{database}}] set single_user with rollback immediate;

drop database [{{database}}];