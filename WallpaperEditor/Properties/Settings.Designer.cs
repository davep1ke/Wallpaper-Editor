﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace WallpaperEditor.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "17.3.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("c:\\backup\\wallpaperEditor")]
        public string backupDirectoryPath {
            get {
                return ((string)(this["backupDirectoryPath"]));
            }
            set {
                this["backupDirectoryPath"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("c:\\owncloud\\wallpapers\\in")]
        public string scanDirectoryPath {
            get {
                return ((string)(this["scanDirectoryPath"]));
            }
            set {
                this["scanDirectoryPath"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n<ArrayOfString xmlns:xsi=\"http://www.w3." +
            "org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">\r\n  <s" +
            "tring>c:\\temp\\type1</string>\r\n  <string>c:\\temp\\type2</string>\r\n</ArrayOfString>" +
            "")]
        public global::System.Collections.Specialized.StringCollection destinationDirectories {
            get {
                return ((global::System.Collections.Specialized.StringCollection)(this["destinationDirectories"]));
            }
            set {
                this["destinationDirectories"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("C:\\OwnCloud\\stuff\\AUTOMA~1\\wallpapers\\convert.bat")]
        public string setWallpaperExePath {
            get {
                return ((string)(this["setWallpaperExePath"]));
            }
            set {
                this["setWallpaperExePath"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("16")]
        public int screenResXmultiplier {
            get {
                return ((int)(this["screenResXmultiplier"]));
            }
            set {
                this["screenResXmultiplier"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("9")]
        public int screenResYmultiplier {
            get {
                return ((int)(this["screenResYmultiplier"]));
            }
            set {
                this["screenResYmultiplier"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("c:\\temp\\testwp.jpg")]
        public string testImageLocation {
            get {
                return ((string)(this["testImageLocation"]));
            }
            set {
                this["testImageLocation"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("1920")]
        public int screenResMinX {
            get {
                return ((int)(this["screenResMinX"]));
            }
            set {
                this["screenResMinX"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("c:\\backup\\discards")]
        public string throwawayFolder {
            get {
                return ((string)(this["throwawayFolder"]));
            }
            set {
                this["throwawayFolder"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("C:\\Program Files (x86)\\paint.net.dontupdate\\PaintDotNet.exe")]
        public string externalEditor {
            get {
                return ((string)(this["externalEditor"]));
            }
            set {
                this["externalEditor"] = value;
            }
        }
    }
}
