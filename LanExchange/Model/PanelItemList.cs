﻿using System;
using System.Collections.Generic;
using System.Text;
using LanExchange.Model.Panel;
using LanExchange.Utils;

//using NLog;

namespace LanExchange.Model
{
    //public enum PanelItemType
    //{
    //    COMPUTERS = 0,
    //    SHARES = 1,
    //    FILES = 2
    //}

    public class PanelItemList : ISubscriber, IEquatable<PanelItemList>, IFilterModel
    {
        //private readonly static Logger logger = LogManager.GetCurrentClassLogger();

        // items added by user
        private readonly IList<AbstractPanelItem> m_Items;
        // merged all results and user items
        private readonly IDictionary<IComparable,AbstractPanelItem> m_Data;
        // keys for filtering
        private readonly IList<IComparable> m_Keys;
        // current path for item list
        private readonly ObjectPath m_CurrentPath;

        public event EventHandler Changed;
        public event EventHandler SubscriptionChanged;

        //private ListView m_LV = null;
        //private PanelItemType m_CurrentType = PanelItemType.COMPUTERS;
        //private string m_Path = null;

        public PanelItemList(string name)
        {
            m_Items = new List<AbstractPanelItem>();
            m_Data = new Dictionary<IComparable, AbstractPanelItem>();
            m_Keys = new List<IComparable>();
            Groups = new List<ISubject>();
            m_CurrentPath = new ObjectPath();
            TabName = name;
            CurrentView = System.Windows.Forms.View.Details;
            ScanMode = false;
        }

        public ObjectPath CurrentPath
        {
            get { return m_CurrentPath; }
        }

        public TabSettings Settings
        {
            // TODO: uncomment ScanGroups here!
            get
            {
                var Page = new TabSettings { 
                    Name = TabName, 
                    Filter = FilterText, 
                    CurrentView = CurrentView, 
                    ScanMode = ScanMode, 
                    //ScanGroups = Groups 
                };
                return Page;
            }
            set
            {
                TabName = value.Name;
                FilterText = value.Filter;
                CurrentView = value.CurrentView;
                ScanMode = value.ScanMode;
                //Groups = value.ScanGroups;
            }
            
        }

        public string TabName { get; set; }

        public System.Windows.Forms.View CurrentView { get; set; }

        //public IDictionary<string, PanelItem> Items
        //{
        //    get { return m_Items; }
        //}

        public bool ScanMode { get; set; }

        public IList<ISubject> Groups { get; set; }

        //public IList<string> Keys
        //{
        //    get { return m_Keys; }
        //}

        //public string FocusedItem { get; set; }

        public void UpdateSubsctiption()
        {
            switch (ScanMode)
            {
                case true:
                    PanelSubscription.Instance.UnSubscribe(this);
                    foreach(var group in Groups)
                        PanelSubscription.Instance.SubscribeToSubject(this, group);
                    break;
                default:
                    PanelSubscription.Instance.UnSubscribe(this);
                    break;
            }
            if (SubscriptionChanged != null)
                SubscriptionChanged(this, EventArgs.Empty);
        }


        public void Add(AbstractPanelItem comp)
        {
            if (comp == null)
                throw new ArgumentNullException("comp");
            if (!m_Data.ContainsKey(comp[0]))
                m_Data.Add(comp[0], comp);
        }

        //TODO: add delete item
        //public void Delete(PanelItem comp)
        //{
        //    m_Data.Remove(comp.Name);
        //}

        public AbstractPanelItem GetAt(int index)
        {
            return Get(m_Keys[index].ToString());
        }

        public AbstractPanelItem Get(string key)
        {
            if (key == null) return null;
            AbstractPanelItem result;
            if (m_Data.TryGetValue(key, out result))
                return result;
            return null;
        }

        //public void Clear()
        //{
        //    m_Data.Clear();
        //}

        private static bool GoodForFilter(string[] strList, string filter1, string filter2)
        {
            for (int i = 0; i < strList.Length; i++)
            {
                if (i == 0)
                {
                    if (PuntoSwitcher.RussianContains(strList[i], filter1) || (PuntoSwitcher.RussianContains(strList[i], filter2)))
                        return true;
                } else
                if (filter1 != null && strList[i].Contains(filter1) || filter2 != null && strList[i].Contains(filter2))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// IFilterModel.FilterText
        /// </summary>
        public String FilterText { get; set; }

        /// <summary>
        /// IFilterModel.AppliFilter()
        /// </summary>
        public void ApplyFilter()
        {
            bool bFiltered = !String.IsNullOrEmpty(FilterText);
            if (!bFiltered)
                FilterText = String.Empty;
            m_Keys.Clear();
            string Filter1 = FilterText.ToUpper();
            string Filter2 = PuntoSwitcher.Change(FilterText);
            if (Filter2 != null) Filter2 = Filter2.ToUpper();
            foreach (var Pair in m_Data)
            {
                string[] A = Pair.Value.GetStringsUpper();
                if (!bFiltered || String.IsNullOrEmpty(Pair.Value[0].ToString()) || GoodForFilter(A, Filter1, Filter2))
                    m_Keys.Add(Pair.Value[0]);
            }
        }

        // Возвращает количество компов в списке
        public int Count
        {
            get { return m_Data.Count; }
        }

        // Возвращает число записей в фильтре
        public int FilterCount
        {
            get { return m_Keys.Count; }
        }

        //    PanelItemComparer comparer = new PanelItemComparer();
        //    Result.Sort(comparer);
        //    return Result;
        //}

        // TODO uncomment ListView_GetSelected
        //public List<string> ListView_GetSelected(ListView LV, bool bAll)
        //{
        //    List<string> Result = new List<string>();
        //    if (LV.FocusedItem != null)
        //        Result.Add(LV.FocusedItem.Text);
        //    else
        //        Result.Add("");
        //    if (bAll)
        //        for (int index = 0; index < LV.Items.Count; index++)
        //            Result.Add(m_Keys[index]);
        //    else
        //        foreach (int index in LV.SelectedIndices)
        //            Result.Add(m_Keys[index]);
        //    return Result;
        //}
        
        // TODO uncomment ListView_SetSelected
        //public void ListView_SetSelected(ListView LV, List<string> SaveSelected)
        //{
        //    LV.SelectedIndices.Clear();
        //    LV.FocusedItem = null;
        //    if (LV.VirtualListSize > 0)
        //    {
        //        for (int i = 0; i < SaveSelected.Count; i++)
        //        {
        //            int index = m_Keys.IndexOf(SaveSelected[i]);
        //            if (index == -1) continue;
        //            if (i == 0)
        //            {
        //                LV.FocusedItem = LV.Items[index];
        //                //LV.EnsureVisible(index);
        //            }
        //            else
        //                LV.SelectedIndices.Add(index);
        //        }
        //    }
        //}


        //public List<string> ToList()
        //{
        //    List<string> Result = new List<string>();
        //    foreach (var Pair in m_Data)
        //        Result.Add(Pair.Value.Name);
        //    return Result;
        //}

        /// <summary>
        /// ISubsctiber.DataChanged implementation.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="subject"></param>
        public void DataChanged(ISubscription sender, ISubject subject)
        {
            //lock (m_Data)
            {
                m_Data.Clear();
                if (subject != ConcreteSubject.Empty)
                {
                    if (ScanMode)
                        foreach (var group in Groups)
                        {
                            foreach (AbstractPanelItem PItem in sender.GetListBySubject(group))
                            {
                                if (!m_Data.ContainsKey(PItem[0]))
                                    m_Data.Add(PItem[0], PItem);
                            }
                        }
                    ;
                    foreach (var item in m_Items)
                        Add(item);
                }
                //lock (m_Keys)
                {
                    ApplyFilter();
                }
            }
            OnChanged();
        }
 
        private void OnChanged()
        {
            if (Changed != null)
                Changed(this, EventArgs.Empty);
        }

        /// <summary>
        /// Возвращает список элементов с верхнего уровня из стека переходов.
        /// В частности это будет список копьютеров, даже если мы находимся на уровне списка ресуров.
        /// </summary>
        /// <returns></returns>
        public IList<AbstractPanelItem> GetTopItemList()
        {
            return null;
            /*
            if (InternalStack.Count == 0)
                return InternalItems;
            else
            {
                IList<PanelItem>[] Arr = InternalStack.ToArray();
                return Arr[0];
            }
             */
        }

        public void LevelDown()
        {
            /*
            if (LV == null || LV.FocusedItem == null)
                return;
            string FocusedText = LV.FocusedItem.Text;
            if (String.IsNullOrEmpty(FocusedText))
            {
                LevelUp();
                return;
            }

            switch (ViewType)
            {
                case LVType.COMPUTERS:
                    if (LV.FocusedItem == null)
                        break;
                    // останавливаем поток пингов
                    MainForm.GetInstance().CancelCompRelatedThreads();
                    // сбрасываем фильтр
                    MainForm.GetInstance().UpdateFilter(MainForm.GetInstance().GetActiveListView(), "", false);
                    // текущий список добавляем в стек
                    //if (InternalItems == null)
                    //    InternalItems = InternalItemList.ToList();
                    InternalStack.Push(InternalItems);
                    // получаем новый список объектов, в данном случае список ресурсов компа
                    InternalItems = PanelItemList.EnumNetShares(FocusedText);
                    // устанавливаем новый список для визуального компонента
                    CurrentDataTable = InternalItems;
                    if (LV.VirtualListSize > 0)
                    {
                        LV.FocusedItem = LV.Items[0];
                        LV.SelectedIndices.Add(0);
                    }
                    // меняем колонки в ListView
                    Path = @"\\" + FocusedText;
                    ViewType = LVType.SHARES;
                    break;
                case LVType.SHARES:
                    MainForm.GetInstance().mFolderOpen_Click(MainForm.GetInstance().mFolderOpen, EventArgs.Empty);
                    break;
                case LVType.FILES:
                    break;
            }
             */
        }

        public void LevelUp()
        {
            /*
            if (InternalStack.Count == 0)
                return;

            //TPanelItem PItem = null;
            string CompName = null;
            if (InternalItemList.Count > 0)
            {
                CompName = Path;
                if (CompName.Length > 2 && CompName[0] == '\\' && CompName[1] == '\\')
                    CompName = CompName.Remove(0, 2);
            }

            InternalItems = InternalStack.Pop();

            
            switch (CurrentType)
            {
                case LVType.COMPUTERS:
                    break;
                case LVType.SHARES:
                    ViewType = LVType.COMPUTERS;
                    break;
                case LVType.FILES:
                    ViewType = LVType.SHARES;
                    break;
            }
            CurrentDataTable = InternalItems;
            InternalItemList.ListView_SelectComputer(MainForm.GetInstance().lvComps, CompName);

            MainForm.GetInstance().UpdateFilter(MainForm.GetInstance().GetActiveListView(), MainForm.GetInstance().eFilter.Text, true);
             */
        }

        public bool Equals(PanelItemList other)
        {
            return String.Compare(TabName, other.TabName, StringComparison.OrdinalIgnoreCase) == 0;
        }

        public string GetTabToolTip()
        {
            StringBuilder sb = new StringBuilder();
            if (ScanMode)
            {
                sb.Append("Обзор сети: ");
                for (int i = 0; i < Groups.Count; i++)
                {
                    if (i > 0)
                        sb.Append(", ");
                    sb.Append(Groups[i]);
                }
                sb.Append(".");
            }
            else
                sb.Append("Обзор сети отключен.");
            return sb.ToString();
        }
    }
}
