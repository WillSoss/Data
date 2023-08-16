if not exists (select 1 from sys.schemas WHERE name = 'cfg')
begin
    exec('create schema cfg;');
end;
go

if not exists (select 1 from sys.tables where name='migrations' and type='U' AND schema_id = schema_id('cfg'))
begin

    create table cfg.migration (
        major int not null,
        minor int not null,
        build int not null,
        rev int not null,
        [description] varchar(100) not null default (''),
        applied_at datetime not null constraint df_migration_applied_at default (getutcdate()),
        constraint pk_migration primary key clustered (major, minor, build, rev)
    );

    insert into cfg.migration values (0, 0, 0, 0, 'Database Created');

end;
go
