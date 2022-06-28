    namespace RainstormTech.Storm.ImageProxy
    {
        public class ResizeImagePayload
        {
            public string nameIn { get;set; }    
            public string containerIn { get;set; }    
            public string containerOut { get;set; }    
            public string connectionString { get;set; }

            public ResizeImagePayload() {
                this.nameIn = "";
                this.containerIn = "";
                this.containerOut = "";
                this.connectionString = "";
            }
        }
    }
    