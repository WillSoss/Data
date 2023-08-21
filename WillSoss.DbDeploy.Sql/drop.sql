if exists (select 1 from sys.sysdatabases where name = '{{database}}')
begin
	alter database [{{database}}] set single_user with rollback immediate;

	drop database [{{database}}];
end;