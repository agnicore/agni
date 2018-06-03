using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agni.WebManager.Controls
{
  interface ISitePage{}

    interface IMainPage : ISitePage{}
      interface IHomePage             : IMainPage{}
      interface IConsolePage          : IMainPage{}
      interface IInstrumentationPage  : IMainPage{}
      interface ITheSystemPage        : IMainPage{}
      interface IProcessManagerPage   : IMainPage{}
}
