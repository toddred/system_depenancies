using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace sysDepends2
{
    class VM : INotifyPropertyChanged
    {
        private Dictionary<string,Node> SoftwareDepends = new Dictionary<string, Node> { };
        private string[] ReadLines;
        private List<string> outMessage = new List<string> { };

        public void LoadFile(string fileName)
        {
            inSource.Clear();
            try
            {
                ReadLines = File.ReadAllLines(fileName);
                foreach (var line in ReadLines)
                    inSource.Add(line);
            }
            catch
            {
                inSource.Add("Cannot read file");
            }
        }
        private ObservableCollection<string> inSource = new ObservableCollection<string> { };
        public ObservableCollection<string> InSource
        {
            get { return inSource; }
            set { inSource = value; NotifyChanged(); }
        }
        private BindingList<string> outSource= new BindingList<string> { };
        public BindingList<string> OutSource
        {
            get { return outSource; }
            set { outSource = value; NotifyChanged(); }
        }
        private void writeToOutput(List<string> _outMessage)
        {
            string _msg ="";
            for (int i = 0; i < _outMessage.Count; i++)
            {
                _msg += "\t"+_outMessage[i];
                if(i < _outMessage.Count-1)
                    _msg += "\n";
            }
            outSource.Add(_msg);
        }
        public void ListIt()
        {
            outMessage.Clear();
            var installedSoftware = SoftwareDepends.Where(x => x.Value.IsInstalled);
            foreach (var softwareitem in installedSoftware)
                outMessage.Add(softwareitem.Key);
            writeToOutput(outMessage);
        }
        public void NotRecognized()
        {
            outSource.Add("Not Recognized, use DEPEND, INSTALL, REMOVE, LIST, or END");
        }
        private KeyValuePair<string,Node> CreateUniqueDependancy(string d, Dictionary<string,Node> sd)
        {
            // if the name isnt in the list
            return !sd.ContainsKey(d) ? new KeyValuePair<string, Node> (d,new Node()) : new KeyValuePair<string, Node>(null,null);
        }
        public void DependsOn(string subject, string dependsOn, Dictionary<string,Node> sd)
        {
            sd[subject].Children.Add(dependsOn);
            sd[dependsOn].Parent.Add(subject);
            SoftwareDepends = sd;
        }
        public void Depend(string[] fileReadItem, Dictionary<string,Node> sd)
        {
            for (int i = 1; i < fileReadItem.Length; i++)
            {
                KeyValuePair<string, Node> temp = CreateUniqueDependancy(fileReadItem[i], sd);
                if(temp.Key != null)
                sd.Add(temp.Key,temp.Value);
            }
           for (int i = 2; i < fileReadItem.Length; i++)
            {
                DependsOn(fileReadItem[1], fileReadItem[i], sd);
            }

            SoftwareDepends = sd;
        }
        private Queue<string> Search(string name, Queue<string> _searchQueue, Queue<string> _actionQueue, Dictionary<string, Node> sd)
        {  
            _actionQueue.Enqueue(name);
             if (_searchQueue.Count < (sd.Count))
              return _actionQueue;
            if (sd[name].Children.Count != 0)
            {
                foreach (string child in sd[name].Children)
                _searchQueue.Enqueue(child);    
            }
            if (_searchQueue.Count > 0)
            {
                Search(_searchQueue.Dequeue(), _searchQueue, _actionQueue, sd);
            }
            return _actionQueue;
        }
        public void Install(string newItem, Dictionary<string, Node> sd)
        {
                outMessage.Clear();
                Queue<string> installQueue = new Queue<string> { };
                Queue<string> searchQueue = new Queue<string> { };

                if (sd.ContainsKey(newItem))
                {
                    if (sd[newItem].IsInstalled)
                    {
                        outMessage.Add($"{newItem} is already installed");
                    }
                    installQueue = Search(newItem, searchQueue, installQueue, sd);
                }
                else
                {
                    var unawareSoftware = CreateUniqueDependancy(newItem, sd);
                    sd.Add(unawareSoftware.Key, unawareSoftware.Value);
                    installQueue.Enqueue(newItem);
                }
                while (installQueue.Count > 0)
                {
                    string queueItem = installQueue.Dequeue();
                    if (!sd[queueItem].IsInstalled)
                    {
                        if (queueItem != newItem)
                        {
                            sd[queueItem].IsImplicit = true;
                        }
                        sd[queueItem].IsInstalled = true;
                        outMessage.Add($"Installing {queueItem}");
                    }
                }
                writeToOutput(outMessage);
        }
        public void Remove(string listItem, Dictionary<string, Node> sd)
        {
            outMessage.Clear();
            Queue<string> actionQueue = new Queue<string> { };
            Queue<string> searchQueue = new Queue<string> { };
            //string[] removeArray;
            List<string> removeList = new List<string> { };
            if (sd[listItem].IsInstalled)
            {
                Search(listItem, searchQueue, actionQueue, sd);
                while (actionQueue.Count > 0)
                {
                    bool hasinstalledparent = false;
                    bool hasparentonremoveL = false;
                    string currentItem = actionQueue.Dequeue();
                    var isInstalled = sd.Where(p => p.Value.IsInstalled);
                    var parentTrap = sd[currentItem].Parent;
                    // bool installedparents = parentTrap.Contains(isInstalled);
                    // does the current item (which is also installed) have an installed parent
                    
                    // is the installed parent on the remove list
                    
                    // who is asking to delete this? the method or the user?

                    foreach (var item in parentTrap)
                    {
                        if (sd[item].IsInstalled)
                        {
                            hasinstalledparent = true;
                            hasparentonremoveL = removeList.Contains(item) ? true : false;
                        }
                    }
                    if (hasinstalledparent)
                    {
                        if(currentItem == listItem)
                        {
                            outMessage.Add($"{listItem} is still needed");
                        }
                        else
                        {
                            if (hasparentonremoveL && sd[currentItem].IsImplicit)
                            {
                                removeList.Add(currentItem);
                            }
                        }
                    }
                    else
                    {
                        removeList.Add(currentItem);
                    }
                }
                foreach (var item in removeList)
                {
                    if (sd[item].IsInstalled)
                    {
                        sd[item].IsInstalled = false;
                        outMessage.Add($"Removing {item}");
                    }                
                }
            }
            else
            {
                outMessage.Add($"{listItem} is not Installed");
            }
            writeToOutput(outMessage);
        }
        public void DoIt()
        {
            foreach (string line in ReadLines)
            {
                string[] t = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                outSource.Add(line);
                switch (t[0])
                {
                    case "END":
                        //End();
                        break;
                    case "DEPEND":
                        Depend(t, SoftwareDepends);
                        break;
                    case "INSTALL":
                        Install(t[1], SoftwareDepends);
                        break;
                    case "REMOVE":
                        Remove(t[1], SoftwareDepends);
                        break;
                    case "LIST":
                        ListIt();
                        break;
                    default:
                        NotRecognized();
                        break;
                }
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
