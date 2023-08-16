if (serverproperty('edition') = N'SQL Azure')
begin
    drop database if exists [{database}];
end;
else
begin
    if not exists (select 1 from sys.sysdatabases where name = '{{database}}')
    begin
        alter database [{{database}}] set single_user with rollback immediate;
        drop database [{{database}}];
    end;
end;