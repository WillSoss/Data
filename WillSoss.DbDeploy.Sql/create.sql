if not exists (select 1 from sys.sysdatabases where name = '{{database}}')
begin
	create database [{{database}}];
end;