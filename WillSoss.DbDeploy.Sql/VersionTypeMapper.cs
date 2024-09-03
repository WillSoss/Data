using Dapper;
using System.Data;

namespace WillSoss.DbDeploy.Sql
{
    internal class VersionTypeMapper : SqlMapper.TypeHandler<Version>
    {
        public override Version Parse(object value) => Version.Parse((string)value);

        public override void SetValue(IDbDataParameter parameter, Version? value)
        {
            throw new NotImplementedException();
        }
    }
}
