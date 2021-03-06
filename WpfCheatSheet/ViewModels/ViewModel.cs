﻿using CheatSheet.Common;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using WpfCheatSheet.Commands;
using WpfCheatSheet.Common;
using WpfCheatSheet.Messengers;
using WpfCheatSheet.Models;

namespace WpfCheatSheet.ViewModels
{
    class ViewModel : ViewModelBase
    {
        public ViewModel()
        {
            if (File.Exists(settingsFile))
            {
                using (var sw = new StreamReader(settingsFile))
                {
                    File1 = sw.ReadLine();
                    Folder1 = sw.ReadLine();
                }
            }

            Decimal1 = 0.0m / 3.0m;

            Items = new ObservableCollection<Item> { new Item("Name1.txt", true, "Body1", "User1",  new DateTime(2017, 1, 1)),
                                                     new Item("Name2.txt", false, "Body2", "User2", new DateTime(2018, 1, 1)),
                                                     new Item("Name3.txt", false, "Body3", "User3", DateTime.Now)};
        }

        const string settingsFile = "Settings.txt";
        public Messenger Messenger { get; set; } = new Messenger();

        public string WindowTitle
        {
            get => string.Concat("Title1 ", U.CreateBreadcrumb());
        }

        string folder1;
        public string Folder1
        {
            get => folder1;

            set
            {
                folder1 = value.Trim();
                NotifyPropertyChanged();
                ErrorIfEmpty(value);
                ErrorIfDirectoryNotFound(value);
            }
        }

        string file1;
        public string File1
        {
            get => file1;

            set
            {
                file1 = value.Trim();
                NotifyPropertyChanged();
                ErrorIfEmpty(value);
                ErrorIfFileNotFound(value);
            }
        }

        decimal decimal1;
        public decimal Decimal1
        {
            get => decimal1;

            set
            {
                decimal1 = value;
                NotifyPropertyChanged();
            }
        }

        BLT selectedBLT;
        public BLT SelectedBLT
        {
            get => selectedBLT;

            set
            {
                selectedBLT = value;
                NotifyPropertyChanged();
            }
        }

        public double DataGridMaxHeight
        {
            get => SystemParameters.VirtualScreenHeight * 0.5;
        }

        ObservableCollection<Item> items;
        public ObservableCollection<Item> Items
        {
            get => items;

            set
            {
                items = value;
                NotifyPropertyChanged();
            }
        }

        Item selectedItem;
        public Item SelectedItem
        {
            get => selectedItem;

            set
            {
                selectedItem = value;
                NotifyPropertyChanged();
            }
        }

        ICommand dropCommand;
        public ICommand DropCommand
        {
            get
            {
                return dropCommand ?? (dropCommand = new DelegateCommand(e =>
                {
                    var paths = ((DragEventArgs)e).Data.GetData(DataFormats.FileDrop) as string[];

                    if (paths == null)
                    {
                        return;
                    }

                    foreach (var path in paths)
                    {
                        if (!Bool.EqIgnoreCase(Path.GetExtension(path), ".txt"))
                        {
                            Messenger.Send(new Message("Please drop a txt file."));
                            return;
                        }

                        var fileName = Path.GetFileName(path);

                        if (Items.Select(r => r.Name).Contains(fileName))
                        {
                            return;
                        }

                        foreach (var item in Items)
                        {
                            item.IsActive = false;
                        }

                        Items.Add(new Item(fileName, true, File.ReadAllText(path), Environment.UserName, DateTime.Now));
                    }
                }));
            }
        }

        ICommand mouseMoveCommand;
        public ICommand MouseMoveCommand
        {
            get
            {
                return mouseMoveCommand ?? (mouseMoveCommand = new DelegateCommand(e =>
                {
                    var mea = (MouseEventArgs)e;

                    if (SelectedItem == null || mea.LeftButton != MouseButtonState.Pressed)
                    {
                        return;
                    }

                    using (var tf = new TemporaryFile(SelectedItem.Name))
                    {
                        File.WriteAllText(tf.Path1, SelectedItem.Body);
                        U.DragDrop1((DependencyObject)mea.Source, tf.Path1);
                    }
                }));
            }
        }

        ICommand openFolderCommand;
        public ICommand OpenFolderCommand
        {
            get
            {
                return openFolderCommand ?? (openFolderCommand = new DelegateCommand(e =>
                {
                    var path = Io.FolderBrowserDialog();

                    if (path == null)
                    {
                        return;
                    }

                    switch (e)
                    {
                        case nameof(Folder1):
                            Folder1 = path;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }));
            }
        }

        ICommand openFileCommand;
        public ICommand OpenFileCommand
        {
            get
            {
                return openFileCommand ?? (openFileCommand = new DelegateCommand(e =>
                {
                    var path = Io.OpenFileDialog();

                    if (path == null)
                    {
                        return;
                    }

                    switch (e)
                    {
                        case nameof(File1):
                            File1 = path;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }));
            }
        }

        ICommand saveSettingsCommand;
        public ICommand SaveSettingsCommand => saveSettingsCommand ?? (saveSettingsCommand =
            new DelegateCommand(e =>
            {
                using (var sw = new StreamWriter(settingsFile))
                {
                    sw.WriteLine();
                    sw.WriteLine();
                }
            }));

        ICommand activateCommand;
        public ICommand ActivateCommand => activateCommand ?? (activateCommand =
            new DelegateCommand(e =>
            {
                if (SelectedItem == null)
                {
                    return;
                }

                foreach (var item in Items)
                {
                    item.IsActive = false;
                }

                SelectedItem.IsActive = true;
            }));

        ICommand deleteCommand;
        public ICommand DeleteCommand => deleteCommand ?? (deleteCommand =
            new DelegateCommand(e =>
            {
                if (SelectedItem == null)
                {
                    return;
                }

                Items.Remove(SelectedItem);

                if (Items.Any() && Items.All(x => !x.IsActive))
                {
                    Items.OrderByDescending(x => x.UpdatedAt).First().IsActive = true;
                }
            }));

        ICommand showSelectedItemCommand;
        public ICommand ShowSelectedItemCommand => showSelectedItemCommand ?? (showSelectedItemCommand =
            new DelegateCommand(e =>
            {
                if (SelectedItem == null)
                {
                    return;
                }

                using (var tf = new TemporaryFile(SelectedItem.Name))
                {
                    File.WriteAllText(tf.Path1, SelectedItem.Body);

                    var p = Process.Start(tf.Path1);

                    if (p != null)
                    {
                        p.WaitForExit();
                    }
                }
            }));
    }
}