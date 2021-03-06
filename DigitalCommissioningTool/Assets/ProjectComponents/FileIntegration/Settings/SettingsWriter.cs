﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using ProjectComponents.Abstraction;
using SystemFacade;
using UnityEngine;

namespace ProjectComponents.FileIntegration
{
    internal class SettingsWriter
    {
        private XmlDocument Doc { get; set; }

        internal SettingsWriter( XmlDocument doc )
        {
            Doc = doc;
        }

        internal void WriteFile( InternalProjectSettings settings )
        {
            LogManager.WriteInfo( "Datei \"Settings.xml\" wird erstellt.", "SettingsWriter", "WriteFile" );

            ReCreateFile( );

            string xmlns = "https://github.com/xShadowArmy/digital-commissioning-tool/tree/main/DigitalCommissioningTool/Output/Resources/";

            try
            {
                Doc.Load( Paths.TempPath + "Settings.xml" );

                XPathNavigator nav = Doc.CreateNavigator( );

                nav.MoveToFirstChild( );


                XmlTextWriter writer = new XmlTextWriter( Paths.TempPath + "Settings.xml", System.Text.Encoding.UTF8 )
                {
                    Formatting = Formatting.Indented,
                    Indentation = 4
                };

                Doc.Save( writer );

                writer.Dispose( );
            }

            catch ( Exception e )
            {
                LogManager.WriteLog( "Datei \"Data.xml\" konnte nicht erstellt werden! Fehler: " + e.Message, LogLevel.Error, true, "DataWriter", "WriteFile" );
            }
        }

        private void ReCreateFile()
        {
            try
            {
                if ( File.Exists( Paths.TempPath + "Settings.xml" ) )
                {
                    File.Delete( Paths.TempPath + "Settings.xml" );
                }

                using ( StreamWriter writer = new StreamWriter( File.Create( Paths.TempPath + "Settings.xml" ) ) )
                {
                    writer.WriteLine( "<?xml version=\"1.0\" encoding=\"utf-8\"?>" );
                    writer.WriteLine( "<xs:ProjectSettings xmlns:xs=\"https://github.com/xShadowArmy/digital-commissioning-tool/tree/main/DigitalCommissioningTool/Output/Resources/\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"https://github.com/xShadowArmy/digital-commissioning-tool/tree/main/DigitalCommissioningTool/Output/Resources/ ProjectSettingsSchema.xsd\">" );
                    writer.WriteLine( "</xs:ProjectSettings>" );

                    writer.Flush( );
                }
            }

            catch ( Exception e )
            {
                LogManager.WriteLog( "Datei \"Data.xml\" konnte nicht erstellt werden! Fehler: " + e.Message, LogLevel.Error, true, "SettingsWriter", "ReCreateFile" );
            }
        }
    }
}