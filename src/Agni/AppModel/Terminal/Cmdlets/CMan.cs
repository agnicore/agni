﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NFX;
using NFX.ApplicationModel;
using NFX.Environment;
using NFX.Glue;

namespace Agni.AppModel.Terminal.Cmdlets
{

    /// <summary>
    /// Component Manager
    /// </summary>
    public class CMan : Cmdlet
    {

        public const string CONFIG_SID_ATTR = "sid";
        public const string CONFIG_NAME_ATTR = "name";
        public const string CONFIG_PARAM_ATTR = "param";
        public const string CONFIG_VALUE_ATTR = "value";


        public static ApplicationComponent GetApplicationComponentBySIDorName(IConfigSectionNode args)
        {
           if (args==null || !args.Exists) return null;

           ApplicationComponent cmp = null;
           var sid = args.AttrByName(CONFIG_SID_ATTR).ValueAsULong();
           var cname = args.AttrByName(CONFIG_NAME_ATTR).Value;

           if (sid>0) cmp = ApplicationComponent.GetAppComponentBySID(sid);

           if (cmp==null && cname.IsNotNullOrWhiteSpace()) cmp = ApplicationComponent.GetAppComponentByCommonName(cname);
           return cmp;
        }


        public CMan(AppRemoteTerminal terminal, IConfigSectionNode args) : base(terminal, args)
        {

        }

        public override string Execute()
        {
            var sid = m_Args.AttrByName(CONFIG_SID_ATTR).ValueAsULong();
            var cname = m_Args.AttrByName(CONFIG_NAME_ATTR).Value;
            var param = m_Args.AttrByName(CONFIG_PARAM_ATTR).Value;
            var value = m_Args.AttrByName(CONFIG_VALUE_ATTR).Value;

            var sb = new StringBuilder(1024);
            sb.AppendLine(AppRemoteTerminal.MARKUP_PRAGMA);
            sb.Append("<push><f color=gray>");
            sb.AppendLine("Component Manager");
            sb.AppendLine("----------------------");

            if (sid==0 && cname.IsNullOrWhiteSpace())
             listAll(sb);
            else
            {
              ApplicationComponent cmp = null;
              if (sid>0) cmp = ApplicationComponent.GetAppComponentBySID(sid);
              if (cmp==null && cname.IsNotNullOrWhiteSpace()) cmp = ApplicationComponent.GetAppComponentByCommonName(cname);
              if (cmp!=null)
                details(sb, cmp, param, value);
              else
                sb.AppendFormat("<f color=red>Component with the supplied SID and CommonName was not found\n");
            }

            sb.AppendLine("<pop>");

            return sb.ToString();
        }





        public override string GetHelp()
        {
  return
@"Prints component information and manages parameters.
            Pass either <f color=yellow>sid<f color=gray> or <f color=yellow>name<f color=gray> for particular component.
            Pass <f color=yellow>param<f color=gray> and <f color=yellow>value<f color=gray> to set the value in particular component.
           Parameters:
            <f color=yellow>sid = int<f color=gray> - component instance SID
         <f color=cyan>or<f color=gray>
            <f color=yellow>name=string<f color=gray> - component common name

            <f color=yellow>param=string<f color=gray> - name of parameter to set
            <f color=yellow>value=object<f color=gray> - new parameter value to set

";
        }


        private void listAll(StringBuilder sb)
        {
           var all = ApplicationComponent.AllComponents;
           var root = all.Where(c => c.ComponentDirector==null);
           sb.AppendLine("<f color=white>Root components w/o director:");
           sb.AppendLine();
           foreach(var cmp in root)
             listOne(sb, all, cmp, 0);

           sb.AppendLine();

           var other = all.Where(c => c.ComponentDirector!=null && !(c.ComponentDirector is ApplicationComponent));
           sb.AppendLine("<f color=white>Components with non-component director:");
           sb.AppendLine();
           foreach(var cmp in other)
             listOne(sb, all, cmp, 0);
        }

        private void listOne(StringBuilder sb, IEnumerable<ApplicationComponent> all, ApplicationComponent cmp, int level)
        {
          if (level>7) return;//cyclical ref

          var sp = level<=0?string.Empty : string.Empty.PadLeft(level*3);
          var pfx0 =  sp+"<f color=darkgray>├▌";
          var pfx =   sp+"<f color=darkgray>├─";
          var pfxL =  sp+"<f color=darkgray>└─";


          sb.AppendLine(pfx0+"<f color={0}>SID: <f color=yellow>{1:D4} <f color=gray>{2} <f color=magenta>{3} <f color=green>{4} "
                                    .Args(
                                          level==0 ? "white" : level>1 ? "darkcyan" : "cyan",
                                          cmp.ComponentSID,
                                          Appl.fdt(cmp.ComponentStartTime),
                                          cmp.ComponentCommonName,
                                          (cmp is INamed ? ((INamed)cmp).Name : "" )));

          if (cmp.ComponentDirector!=null && !(cmp.ComponentDirector is ApplicationComponent))
           sb.AppendLine(pfx+"<f color=gray>Director: <f color=blue>"+cmp.ComponentDirector.GetType().FullName);

          sb.AppendLine(pfxL+"<f color=gray>{0} <f color=darkgray>{1}".Args(cmp.GetType().FullName, System.IO.Path.GetFileName( cmp.GetType().Assembly.Location )));



          var children = all.Where(c => object.ReferenceEquals(c.ComponentDirector, cmp));
          foreach(var child in children)
          {
            listOne(sb, all, child, level+1);
          }

          if (level==0) sb.AppendLine();
        }



        private void details(StringBuilder sb, ApplicationComponent cmp, string param, string value)
        {
          if (param.IsNotNullOrWhiteSpace())
          {
            sb.AppendLine("<f color=gray>Trying to set parameter <f color=yellow>'{0}'<f color=gray> to value  <f color=cyan>'{1}'".Args(param, value ?? "<null>"));
            if (!NFX.ExternalParameterAttribute.SetParameter(cmp, param, value))
            {
             sb.AppendLine("<f color=red>Parameter <f color=yellow>'{0}'<f color=red> set did NOT SUCCEED".Args(param));
             return;
            }
            sb.AppendLine("<f color=green>Parameter <f color=yellow>'{0}'<f color=green> set SUCCEEDED".Args(param));
            sb.AppendLine();
          }

          dumpDetails(sb, cmp, 0);
        }

        private void dumpDetails(StringBuilder sb, ApplicationComponent cmp, int level)
        {
          if (level>7) return;//cyclical ref

          var pfx = level<=0?string.Empty : string.Empty.PadLeft(level)+"->";

          sb.AppendLine(pfx+"<f color=white>SID: <f color=yellow> "+cmp.ComponentSID);
          sb.AppendLine(pfx+"<f color=gray>CommonName: <f color=magenta> "+cmp.ComponentCommonName);
          sb.AppendLine(pfx+"<f color=gray>Start Time (local): <f color=yellow> "+cmp.ComponentStartTime);
          sb.AppendLine(pfx+"<f color=gray>Type: <f color=yellow> "+cmp.GetType().FullName);
          sb.AppendLine(pfx+"<f color=gray>Assembly: <f color=yellow> "+cmp.GetType().Assembly.FullName);
          sb.AppendLine(pfx+"<f color=gray>Service: <f color=yellow> "+(cmp is NFX.ServiceModel.Service ? "Yes" : "No") );
          if (cmp is INamed)
           sb.AppendLine(pfx+"<f color=gray>Name: <f color=green> "+((INamed)cmp).Name);

          sb.AppendLine(pfx+"<f color=gray>Interfaces: <f color=yellow> "+cmp.GetType()
                                                                             .GetInterfaces()
                                                                             .OrderBy(it=>it.FullName)
                                                                             .Aggregate("",(r,i)=>
                                                                                 r+(typeof(IExternallyParameterized).IsAssignableFrom(i) ?
                                                                                      "<f color=cyan>{0}<f color=yellow>".Args(i.Name) : i.Name)+", ") );

          sb.AppendLine();
          sb.AppendLine();
          sb.AppendLine(pfx+"Parameters: ");
          sb.AppendLine();

          var pars = NFX.ExternalParameterAttribute.GetParametersWithAttrs(cmp.GetType());
          foreach(var p in pars)
          {
            var nm = p.Item1;
            object val;
            if (!NFX.ExternalParameterAttribute.GetParameter(cmp, nm, out val)) val = "?";
            var tp = p.Item2;
            var atr = p.Item3;
            sb.AppendLine(pfx+"<f color=gray>{0,-35}: <f color=white>{1,-10} <f color=darkyellow>({2})  <f color=darkgreen>{3}".Args(
                             nm,
                             val==null ? "<null>" : val,
                             tp.DisplayNameWithExpandedGenericArgs().Replace('<','(').Replace('>',')')
                             , atr.Groups==null?"*":atr.Groups.Aggregate("",(r,a)=>r+a+", ")));
          }


          sb.AppendLine();
          var dir = cmp.ComponentDirector;
          sb.AppendLine(pfx+"<f color=gray>Director: <f color=magenta> "+(dir==null? " -none- " : dir.GetType().FullName));
          if (dir is ApplicationComponent)
            dumpDetails(sb, dir as ApplicationComponent, level+1);
        }

    }

}
