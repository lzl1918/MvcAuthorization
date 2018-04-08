using System;
using System.Collections.Generic;
using System.Text;

namespace AuthorizationCore
{

    public interface IPolicy<TUser>
    {

    }
    public interface IPolicy<TUser, TObject>
    {
    }
}
