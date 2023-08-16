if not exists (select 1 from sys.sysdatabases where name = '{{database}}')
begin
    if (serverproperty('edition') = N'SQL Azure')
    begin
        exec('create database [{{database}}] (EDITION = ''basic'')');
    end;
    else
    begin
        exec('create database [{{database}}]');
    end;
end;