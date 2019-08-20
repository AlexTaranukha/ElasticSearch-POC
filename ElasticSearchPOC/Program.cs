using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using dtSearch.Engine;


namespace ElasticSearchPOC
{
    class Program
    {
        static async Task Main(string[] args)
        {
            int numberOfIterations = int.Parse(args[0]);
            int numberOfThreads = int.Parse(args[1]);
            int numberOfDocs = int.Parse(args[2]);
            List<Task> tf = new List<Task>();
            List<Task> td = new List<Task>();

            string EnginePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dtSearchEngine.dll");
            dtSearch.Engine.Server.SetEnginePath(EnginePath);

            try
            {
                int artifactId = 0;
                Console.WriteLine("Elastic Search POC. Number of documents - " + numberOfDocs + ". Number of threads - " + numberOfThreads + ".");
                DateTime startTime = DateTime.Now;
                Console.WriteLine("ECE POC - Reading from file into a memory stream - Start: " + startTime.ToString());
                for (int n = 0; n < numberOfIterations; n++)
                {
                    int docId = 0;
                    POCDataSource ds = new POCDataSource(numberOfDocs);

                    while (docId < numberOfDocs)
                    {
                        for (int i = 0; i < numberOfThreads; i++)
                        {
                            var fileName = "../../../Data/audit" + docId.ToString() + ".json";
                            tf.Add(await Task.Factory.StartNew(async () =>
                                //await ReadFileIntoMemoryStream("smb://p-dv-dsk-zmec0/ECELoad/" + fileName)
                                await ReadFileIntoMemoryStream(artifactId, fileName, ds)
                                ));

                            docId++;
                            artifactId++;
                            //File.Copy("/Users/alex.taranukha/Projects/ElasticSearchPOC/audit.json", "/Users/alex.taranukha/Projects/ElasticSearchPOC/audit" + i.ToString() + ".json");
                        }
                        Task.WaitAll(tf.ToArray());
                        //await Task.WhenAll(tf.ToArray());
                    }

                    BuildIndex("/Users/alex.taranukha/Projects/ElasticSearchPOC/FileIndex/", ds);
              
                }
                DateTime endTime = DateTime.Now;
                Console.WriteLine("ECE POC - Reading from file into a memory stream - End: " + endTime.ToString());
                Console.WriteLine("Total Execution Time - " + (endTime - startTime).ToString());

                artifactId = 0;
                startTime = DateTime.Now;
                Console.WriteLine("ECE POC - Reading from ECE into a memory stream - Start: " + startTime.ToString());
                for (int n = 0; n < numberOfIterations; n++)
                {
                    int docId = 0;
                    POCDataSource ds = new POCDataSource(numberOfDocs);

                    while (docId < numberOfDocs)
                    {
                        for (int i = 0; i < numberOfThreads; i++)
                        {
                            var docName = "http://localhost:9200/audit/tweet/" + docId.ToString();
                            td.Add(await Task.Factory.StartNew(async () =>
                                await ReadECEDocIntoMemoryStream(artifactId, docName, ds)
                                ));
                            docId++;
                            artifactId++;
                        }
                        Task.WaitAll(td.ToArray());
                        //await Task.WhenAll(td.ToArray());
                    }

                    BuildIndex("/Users/alex.taranukha/Projects/ElasticSearchPOC/ECEIndex/", ds);

                }
                endTime = DateTime.Now;
                Console.WriteLine("ECE POC - Reading from ECE into a memory stream - End: " + endTime.ToString());
                Console.WriteLine("Total Execution Time - " + (endTime - startTime).ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            
        }


        async public static Task ReadFileIntoMemoryStream(int aId, String filePath, POCDataSource dataSource)
        {
            try
            {
                //Directory.SetCurrentDirectory(filePath);
                //var file = File.Open(filePath);

                using (FileStream fileStream = new FileStream(filePath, FileMode.Open))
                {
                    MemoryStream memStream = new MemoryStream();
                    await ((Stream)fileStream).CopyToAsync(memStream);
                    //Console.WriteLine("File read - " + filePath + " : " + memStream.Length);

                    dataSource.GetNextDoc();
                    dataSource.DocStream = memStream;
                    dataSource.DocId = aId;
                    dataSource.DocName = filePath;
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        async public static Task ReadECEDocIntoMemoryStream(int aId, String jsonPath, POCDataSource dataSource)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    using (HttpResponseMessage res = await client.GetAsync(jsonPath))
                    using (HttpContent content = res.Content)
                    {
                        MemoryStream memStream = new MemoryStream();
                        await content.CopyToAsync(memStream);
                        //Console.WriteLine("Doc loaded - " +jsonPath + " : " + memStream.Length);

                        dataSource.GetNextDoc();
                        dataSource.DocStream = memStream;
                        dataSource.DocId = aId;
                        dataSource.DocName = jsonPath;

                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }


        public static bool BuildIndex(string ip, DataSource ds)
        {
            bool returnValue;
            using (dtSearch.Engine.IndexJob job = new IndexJob())
            {
                job.IndexPath = ip;
                job.ActionCreate = true;
                job.ActionAdd = true;
                job.ActionMerge = true;
                job.DataSourceToIndex = ds;

                Console.WriteLine("BuildIndex Start - " + DateTime.Now.ToString());
                returnValue = job.Execute();
                if (!returnValue) Console.WriteLine("JOB ERROR - " + job.Errors.ToString());
                Console.WriteLine("BuildIndex End - " + DateTime.Now.ToString());
            }
            return returnValue;
        }

    }

    public class POCDataSource : dtSearch.Engine.DataSource
    {
        private int currentStream;
        private List<int> id;
        private List<string> name;
        private bool isFile = false;
        private List<MemoryStream> stream;
        private DateTime docModification = DateTime.Now;
        private DateTime docCreation = DateTime.Now;
        private string docText;
        private string docFields;
        private int docWordCount;
        private TypeId docTypeId;
        private bool wasDocError;
        private string docError;
        private byte[] docBytes;

        public POCDataSource(int capacity)
        {
            currentStream = -1;
            id = new List<int>(capacity);
            name = new List<string>(capacity);
            stream = new List<MemoryStream>(capacity);
            for (int i=0; i<capacity; i++)
            {
                id.Insert(i, 0);
                name.Insert(i, "");
                stream.Insert(i, null);
            }
        }


        public int Id { get => id[currentStream]; set => id.Insert(currentStream, value); }
        public string DocName { get => name[currentStream]; set => name.Insert(currentStream, value); }
        public bool DocIsFile { get => isFile; set => isFile = value; }
        public string DocDisplayName { get => name[currentStream]; set => name.Insert(currentStream, value); }
        public DateTime DocModifiedDate { get => docModification; set => docModification = value; }
        public DateTime DocCreatedDate { get => docCreation; set => docCreation = value; }
        public string DocText { get => docText; set => docText = value; }
        public string DocFields { get => docFields; set => docFields = value; }
        public int DocId { get => id[currentStream]; set => id.Insert(currentStream, value); }
        public int DocWordCount { get => docWordCount; set => docWordCount = value; } 
        public TypeId DocTypeId { get => TypeId.it_DocFile; set => docTypeId = value; }
        public bool WasDocError { get => wasDocError; set => wasDocError = value; }
        public string DocError { get => docError; set => docError = value; }
        public Stream DocStream { get => stream[currentStream]; set => stream.Insert(currentStream, (MemoryStream)value); }
        public byte[] DocBytes { get => docBytes; set => docBytes = value; }

        public bool GetNextDoc()
        {
            if ((++currentStream >= stream.Count) && (currentStream !=0)) return false;
            return true;
        }

        public bool Rewind()
        {
            currentStream = -1;
            return true;
        }
    }
}
