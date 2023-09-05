if not exists (select 1 from sys.schemas WHERE name = 'cfg')
begin
    exec('create schema cfg;');
end;
go

if not exists (select 1 from sys.tables where name='migration' and type='U' AND schema_id = schema_id('cfg'))
begin

    create table cfg.migration (
        major int not null,
        minor int not null,
        build int not null,
        rev int not null,
        phase int not null,
        number int not null,
        [description] varchar(100) not null default (''),
        applied_at datetimeoffset not null constraint df_migration_applied_at default (sysutcdatetime()),
        constraint pk_migration primary key clustered (major, minor, build, rev, phase, number)
    );

    insert into cfg.migration (major, minor, build, rev, phase, number, [description], applied_at) values 
        (0, 0, -1, -1, 0, 0, 'Database Created', sysutcdatetime());

end;
go

create or alter view cfg.migration_detail as
    select  concat(major, '.', minor, iif(build >= 0, concat('.', build, iif(rev >= 0, concat('.', rev), '')), '')) [version], 
            phase,
            number,
            [description], 
            applied_at
    from    cfg.migration;

go