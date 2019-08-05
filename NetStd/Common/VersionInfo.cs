using System;


namespace dtSearch.Sample
    {
    public class VersionInfo
        {
        public string EngineVersion { set; get; }
        public int EngineMajorVersion { set; get; }
        public int EngineMinorVersion { set; get; }
        public int EngineBuild { set; get; }
        public string LoadError { set; get; }
        public bool LoadedOK { set; get; }
        public string PlatformString { set; get; }

        public override string ToString() {
            if (LoadedOK)
                return EngineVersion;
            else
                return LoadError;
            }

        public VersionInfo() {
            LoadError = "";
            GetEngineVersion();
            GetPlatformInfo();
            }

        private void GetPlatformInfo() {
            string env = "";

            try {
                env = "Platform: " + Environment.OSVersion.VersionString;
                if (Environment.Is64BitProcess)
                    env = env + " 64-bit process, ";
                else
                    env = env + " 32-bit process, ";
                env = env + " CLR: " + Environment.Version;
                }
            catch (Exception ex) {
                env += " " + ex.Message;
                }
            PlatformString = env;
            }

        private void GetEngineVersion() {
            try {
                dtSearch.Engine.Server server = new dtSearch.Engine.Server();
                EngineMajorVersion = server.MajorVersion;
                EngineMinorVersion = server.MinorVersion;
                EngineBuild = server.Build;
                EngineVersion = server.MajorVersion + "." + server.MinorVersion + " Build " + server.Build;
                LoadedOK = true;
                }
            catch (DllNotFoundException ex) {
                LoadError = "DllNotFoundException: " + ex.Message;
                }
            catch (BadImageFormatException ex) {   // This means that the 32-bit version of dtSearchEngine.dll is being used in a 64-bit process or a 
                                                   // the 64-bit version is being used in a 32-bit process
                LoadError = "BadImageFormatException: " + ex.Message;
                }
            catch (MissingMethodException ex) {
                LoadError = "MissingMethodException: " + ex.Message;
                }
            catch (Exception ex) {
                LoadError = ex.Message;
                }

            }

        }
    }
