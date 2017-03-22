using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XamarinFormsGoogleDriveAPI.Services.Service.Interface
{
    public interface ITokenIssuer<T>
    {
        T GetToken();
    }
}
