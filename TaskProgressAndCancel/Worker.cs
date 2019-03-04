using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskProgressAndCancel
{
    public class Worker
    {
        private List<string> _package;
        private int _index;

        public string CreateItem()
        {
            System.Threading.Thread.Sleep(1000);
            return $"Item nr {(_index++).ToString()} collected." + Environment.NewLine;
        }

        public async Task<string> CreateItemAsync()
        {
            return await Task.Run(() => {
                System.Threading.Thread.Sleep(1000);
                return $"Item nr {(_index++).ToString()} collected." + Environment.NewLine;
            });
            
        }

        public List<string> CreatePackage(int size)
        {
            _package = new List<string>();
            _index = 0;

            for (int i = 0; i < size; i++)
            {
                _package.Add(CreateItem());
            }

            return _package;
        }

        public async Task<List<string>> CreatePackageAsync(int size)
        {
            _package = new List<string>();
            _index = 0;

            for (int i = 0; i < size; i++)
            {
                var item = await CreateItemAsync();
                _package.Add(item);
            }

            return _package;
        }

        public async Task<List<string>> CreatePackageParallelAsync(int size)
        {
            _package = new List<string>();
            _index = 0;

            Task<string>[] tasks = new Task<string>[size];

            for (int i = 0; i < size; i++)
            {
                tasks[i] = CreateItemAsync();
            }

            await Task.WhenAll(tasks).ContinueWith((t) => 
            {

                foreach (var task in tasks)
                {
                    _package.Add(task.Result);
                    
                }
                //for (int i = 0; i < size; i++)
                //    _package.Add(t.GetAwaiter().GetResult());
            });

            return _package;
        }
    }
}
