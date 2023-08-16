if not exists (select 1 from sys.sysdatabases where name = '{{database}}')
begin
    if (serverproperty('edition') = N'SQL Azure')
    begin
        create database [{{database}}] ( edition = 'basic');
    end;
    else
    begin
        create database [{{database}}];
    end;
end;