using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NFX;
using NFX.ApplicationModel;
using NFX.Security;
using NFX.Wave;

namespace Agni.WebManager
{
  /// <summary>
    /// Represents AWM-specific web session on WAVE server
    /// </summary>
    [Serializable]
    public class AWMWebSession : WaveSession
    {
        protected AWMWebSession() : base(){} //used by serializer
        public AWMWebSession(Guid id) : base(id) {}


        /// <summary>
        /// Returns language code for session - defaulted from geo-location
        /// </summary>
        public override string LanguageISOCode
        {
          get
          {
            string lang = null;

            if (GeoEntity!=null && GeoEntity.Location.HasValue)
            {
                var country = GeoEntity.CountryISOName;
                if (country.IsNotNullOrWhiteSpace())
                 lang = Localizer.CountryISOCodeToLanguageISOCode(country);
            }

            if (lang.IsNullOrWhiteSpace())
             lang = Localizer.ISO_LANG_ENGLISH;

            return lang;
          }
        }
    }
}
