using Caliburn.Micro;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using OpenFMSL.Contracts.Entities;
using OpenFMSL.Contracts.Infrastructure.Messaging;
using OpenFMSL.Contracts.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonProjectStorage
{
    public class JsonProjectStorage : IProjectStorage, IHandle<StoreEntityInRepositoryMessage>, IHandle<OpenRepositoryMessage>
    {
        private readonly IEventAggregator _aggregator;
        public JsonProjectStorage(IEventAggregator aggregator)
        {
            _aggregator = aggregator;
            _aggregator.Subscribe(this);
        }

        public void Handle(StoreEntityInRepositoryMessage message)
        {
            StoreProjectToFile(message.Filename, message.Data);
            //    _aggregator.PublishOnUIThread(new LogMessage { TimeStamp = DateTime.Now, Sender = this, Channel = LogChannels.Information, MessageText = "I stored the project in file " + message.Filename });
        }

        void StoreProjectToFile(string filename, Entity root)
        {
            var data = JsonConvert.SerializeObject(root,
              Formatting.Indented,
              new JsonSerializerSettings()
              {
                  PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                  TypeNameHandling = TypeNameHandling.All,

                  ObjectCreationHandling = ObjectCreationHandling.Replace
              });
            File.WriteAllText(filename, data);

        }

        void RestoreProjectFromFile(string filename)
        {
            Entity deserialized = null;

            var text = System.IO.File.ReadAllText(filename);
            deserialized = JsonConvert.DeserializeObject<Entity>(text, new JsonSerializerSettings()
            {
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                TypeNameHandling = TypeNameHandling.All,
                ObjectCreationHandling = ObjectCreationHandling.Replace
            }
                );


            if (deserialized != null)
            {               
                _aggregator.PublishOnUIThread(new RestoreProjectFromRepositoryMessage() { RestoredData = deserialized });
            }
        }
        public void Handle(OpenRepositoryMessage message)
        {
            if (File.Exists(message.Filename))
            {
                RestoreProjectFromFile(message.Filename);
            }
            else
            {
                _aggregator.PublishOnUIThread(new LogMessage { TimeStamp = DateTime.Now, Sender = this, Channel = LogChannels.Error, MessageText = "File " + message.Filename + " not found" });
            }
        }
    }


}
