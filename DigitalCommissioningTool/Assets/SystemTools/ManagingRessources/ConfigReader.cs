﻿using System;
using System.Xml;
using System.Xml.XPath;
using SystemTools.Logging;

namespace SystemTools.ManagingRessources
{
    internal class ConfigReader
    {
        private XmlDocument Doc { get; set; }

        internal ConfigReader( XmlDocument doc, ConfigManager.ConfigBuffer buffer, bool newFile )
        {
            Doc = doc;

            if ( newFile )
            {
                return;
            }

            FillBuffer( buffer );
        }
        
        internal ConfigData LoadData( ConfigManager.ConfigBuffer buffer, string name )
        {
            for( int i = 0; i < buffer.Data.Count; i++ )
            {
                if ( buffer.Data[i].Name.Equals( name ) )
                {
                    return buffer.Data[ i ];
                }
            }

            return null;
        }

        internal ConfigData LoadData( ConfigManager.ConfigBuffer buffer, long id )
        {
            for ( int i = 0; i < buffer.Data.Count; i++ )
            {
                if ( buffer.Data[ i ].ID == id )
                {
                    return buffer.Data[ i ];
                }
            }

            return null;
        }

        public void LoadData( ConfigManager.ConfigBuffer buffer, string name, ISerialConfigData data )
        {
            ConfigData cd = null;
            SerialConfigData scd;

            for ( int i = 0; i < buffer.Data.Count; i++ )
            {
                if ( buffer.Data[ i ].Name.Equals( name ) )
                {                    
                    cd = buffer.Data[ i ];

                    break;
                }
            }

            if ( cd == null || !cd.IsObject )
            {
                return;
            }

            scd = SerialConfigData.Initialize( );

            for( int i = 0; i < cd.ArrayLength; i++ )
            {
                SerialConfigData.AddData( scd, cd.GetValuesAsString( )[i], cd.Types[ i ] );
            }

            data.Restore( scd );
        }

        private void FillBuffer( ConfigManager.ConfigBuffer buffer )
        {
            LogManager.WriteInfo( "Einlesen von ConfigDaten", "ConfigReader", "FillBuffer" );

            string xmlns = "https://github.com/xShadowArmy/digital-commissioning-tool/tree/main/DigitalCommissioningTool/Output/Ressources/Data";

            XPathNavigator nav = Doc.CreateNavigator( );

            try
            {
                if ( nav.MoveToFirstChild() )
                {                 
                    if ( !long.TryParse( nav.GetAttribute( "dataCount", xmlns ), out long dataCount ) )
                    {
                        LogManager.WriteLog( "Fehler beim Einlesen der ConfigDaten! DataCount attribut ist keine ganze zahl!", LogLevel.Error, true, "ConfigReader", "FilBuffer" );
                    }

                    if ( dataCount > 0 )
                    {
                        if ( !nav.MoveToFirstChild( ) )
                        {
                            LogManager.WriteLog( "Fehler beim Einlesen der ConfigDaten! DataCount entspricht nicht der Anzahl an Daten!", LogLevel.Warning, false, "ConfigReader", "FilBuffer" );
                        }
                    }

                    for ( long i = 0; i < dataCount; i++ )
                    {
                        switch ( nav.LocalName )
                        {
                            case "simpleData":

                                ReadSimpleElement( nav, buffer, xmlns );
                                break;

                            case "complexData":

                                ReadComplexElement( nav, buffer, xmlns );
                                break;

                            default:

                                LogManager.WriteLog( "Fehler beim Einlesen der ConfigDaten! Unbekanntes Datenelement!", LogLevel.Warning, false, "ConfigReader", "FilBuffer" );
                                break;
                        }

                        nav.MoveToNext( );
                    }
                }

                else
                {
                    LogManager.WriteLog( "Fehler beim Einlesen der ConfigDaten! 'Config' Element konnte nicht gefunden werden!", LogLevel.Error, true, "ConfigReader", "FilBuffer" );
                }
            }

            catch( Exception e )
            {
                LogManager.WriteLog( "Fehler beim Einlesen der ConfigDaten! Fehler: " + e.Message, LogLevel.Error, true, "ConfigReader", "FilBuffer" );
            }
        }

        private void ReadSimpleElement( XPathNavigator nav, ConfigManager.ConfigBuffer buffer, string xmlns )
        {
            LogManager.WriteLog( "Einlesen von simpleData Element", LogLevel.Info, false, "ConfigReader", "ReadSimpleElement" );

            try
            {
                string name = nav.GetAttribute( "name", xmlns );
                string type = nav.GetAttribute( "type", xmlns );
                string[ ] values;

                if ( name.Equals( string.Empty ) )
                {
                    LogManager.WriteLog( "Fehler beim Einlesen der ConfigDaten! Konnte 'name' nicht lesen! ", LogLevel.Error, true, "ConfigReader", "ReadSimpleElement" );
                }

                if ( type.Equals( string.Empty ) )
                {
                    LogManager.WriteLog( "Fehler beim Einlesen der ConfigDaten! Konnte 'type' nicht lesen! ", LogLevel.Error, true, "ConfigReader", "ReadSimpleElement" );
                }

                if ( !long.TryParse( nav.GetAttribute( "id", xmlns ), out long id ) )
                {
                    LogManager.WriteLog( "Fehler beim Einlesen der ConfigDaten! Konnte 'id; nicht lesen! ", LogLevel.Error, true, "ConfigReader", "ReadSimpleElement" );
                }

                if ( !long.TryParse( nav.GetAttribute( "elementCount", xmlns ), out long elementCount ) )
                {
                    LogManager.WriteLog( "Fehler beim Einlesen der ConfigDaten! Konnte 'elementCount' nicht lesen! ", LogLevel.Error, true, "ConfigReader", "ReadSimpleElement" );
                }

                if ( !bool.TryParse( nav.GetAttribute( "isArray", xmlns ), out bool isArray ) )
                {
                    LogManager.WriteLog( "Fehler beim Einlesen der ConfigDaten! Konnte 'isArray' nicht lesen! ", LogLevel.Error, true, "ConfigReader", "ReadSimpleElement" );
                }

                ConfigData data = ConfigData.Initialize( );

                values = new string[ elementCount ];

                if ( !nav.MoveToFirstChild() )
                {
                    LogManager.WriteLog( "Fehler beim Einlesen der ConfigDaten! Daten angelegt aber kein Wert! ", LogLevel.Error, true, "ConfigReader", "ReadSimpleElement" );
                }
                
                for( long i = 0; i < elementCount; i++ )
                {
                    values[ i ] = nav.Value;

                    nav.MoveToNext( );
                }

                nav.MoveToParent( );

                data.AddData( name, id, type, isArray, false, values );
                buffer.Data.Add( data );
            }

            catch( Exception e )
            {
                LogManager.WriteLog( "Fehler beim Einlesen der ConfigDaten! Konnte Simples Datenelement nicht lesen! Fehler: " + e.Message, LogLevel.Error, true, "ConfigReader", "ReadSimpleElement" );
            }
        }

        private void ReadComplexElement( XPathNavigator nav, ConfigManager.ConfigBuffer buffer, string xmlns )
        {
            LogManager.WriteLog( "Einlesen von ComplexData Element", LogLevel.Info, false, "ConfigReader", "ReadComplexElement" );
        
            try
            {
                string name = nav.GetAttribute( "name", xmlns );
                string type = nav.GetAttribute( "type", xmlns );
                string[ ] values;
                string[ ] types;

                if ( name.Equals( string.Empty ) )
                {
                    LogManager.WriteLog( "Fehler beim Einlesen der ConfigDaten! Konnte 'name' nicht lesen! ", LogLevel.Error, true, "ConfigReader", "ReadComplexElement" );
                }

                if ( !type.Equals( "objectData" ) )
                {
                    LogManager.WriteLog( "Fehler beim Einlesen der ConfigDaten! Falscher Type fuer complexData! ", LogLevel.Error, true, "ConfigReader", "ReadComplexElement" );
                }

                if ( !long.TryParse( nav.GetAttribute( "id", xmlns ), out long id ) )
                {
                    LogManager.WriteLog( "Fehler beim Einlesen der ConfigDaten! Konnte 'id; nicht lesen! ", LogLevel.Error, true, "ConfigReader", "ReadComplexElement" );
                }

                if ( !nav.MoveToFirstChild() )
                {
                    LogManager.WriteLog( "Fehler beim Einlesen der ConfigDaten! 'Values' element nicht vorhanden! ", LogLevel.Error, true, "ConfigReader", "ReadComplexElement" );
                }

                if ( !long.TryParse( nav.GetAttribute( "propertyCount", xmlns ), out long propertyCount ) )
                {
                    LogManager.WriteLog( "Fehler beim Einlesen der ConfigDaten! Konnte 'propertyCount; nicht lesen! ", LogLevel.Error, true, "ConfigReader", "ReadComplexElement" );
                }

                ConfigData data = ConfigData.Initialize( );

                values = new string[ propertyCount ];
                types  = new string[ propertyCount ];

                if ( !nav.MoveToFirstChild( ) )
                {
                    LogManager.WriteLog( "Fehler beim Einlesen der ConfigDaten! complexElement hat keinen Wert! ", LogLevel.Error, true, "ConfigReader", "ReadComplexElement" );
                }

                for ( long i = 0; i < propertyCount; i++ )
                {
                    values[ i ] = nav.GetAttribute( "value", xmlns );
                    types[ i ]  = nav.GetAttribute( "type", xmlns );

                    nav.MoveToNext( );
                }

                nav.MoveToParent( );
                nav.MoveToParent( );

                data.AddData( name, id, types, false, values );
                buffer.Data.Add( data );
            }

            catch ( Exception e )
            {
                LogManager.WriteLog( "Fehler beim Einlesen der ConfigDaten! Konnte Simples Datenelement nicht lesen! Fehler: " + e.Message, LogLevel.Error, true, "ConfigReader", "ReadComplexElement" );
            }
        }
    }
}
