using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectModel<T> : IMonoBehaviour, IPageHandler
{
    public int Page { get; protected set; }
    public int LastPage => GetLastPage();

    [SerializeField] protected int selectCount = 1;
    [SerializeField] protected int capacity = 4;
    [SerializeField] protected PageMode pageMode = PageMode.NewPage;
    [SerializeField] protected List<T> storage = new List<T>();
    protected List<T> SortedStorage => filter.Sort(storage).ToList();
    protected List<T> ResultStorage => filter.Filter(SortedStorage).ToList();
    protected IFilter<T> filter = new IFilter<T>();
    

    protected ISelectableArray<T> selectableArray = new ISelectableArray<T>(4);
    public bool[] IsSelected => selectableArray.IsSelected;
    public T[] Selections => selectableArray.Items;
    public T[] CurrentSelectedItems => selectableArray.CurrentSelectItems;
    public int[] Cursor => selectableArray.CurrentSelectIndex;
    public int Count => ResultStorage.Count;
    public int SelectionSize => selectableArray.Size;
    public int SelectionCapacity => capacity;


    protected override void Awake() {
        base.Awake();
        selectableArray.SetCapacity(capacity);
        selectableArray.SetSelectCount(selectCount);
        SetPage(0);
    }

    public virtual int GetLastPage() {
        switch (pageMode) {
            default:
            case PageMode.NewPage:
                return (ResultStorage.Count - 1) / selectableArray.Capacity;
            case PageMode.Indent:
                return Mathf.Max(0, ResultStorage.Count - selectableArray.Capacity);
        }
    }

    public virtual void SetPage(int newPage) {
        if (!newPage.IsWithin(0, LastPage))
            return;

        Page = newPage;
        T[] newSelections = ResultStorage.Where((x, i) => PageFilter(i)).ToArray();
        SetSelections(newSelections);
    }

    protected virtual bool PageFilter(int index) {
        return index.IsInRange(GetResultStorageIndex(0), GetResultStorageIndex(selectableArray.Capacity));
    }

    protected virtual int GetResultStorageIndex(int selectIndex) {
        switch (pageMode) {
            default:
            case PageMode.NewPage:
                return Page * selectableArray.Capacity + selectIndex;
            case PageMode.Indent:
                return Page + selectIndex;
        }
    }

    public virtual void PrevPage() {
        SetPage(Page - 1);
    }

    public virtual void NextPage() {
        SetPage(Page + 1);
    }

    public virtual void SetStorage(List<T> storage, int defaultSelectPage = 0) {
        this.storage = storage;
        Reset(defaultSelectPage);
    }

    protected virtual void SetSelections(T[] selections) {
        selectableArray.SetArray(selections, selections == this.Selections);
    }

    protected virtual void SetSelectionsCapacity(int capacity) {
        selectableArray.SetCapacity(capacity);
    }


    public virtual void Reset(int defaultSelectPage = 0) {
        filter.Reset();
        SetPage(defaultSelectPage);
    }

    public virtual void Select(int index) {
        if (!index.IsInRange(0, capacity))
            return;

        selectableArray.Select(index);
    }

    public virtual void SelectAll(bool active) {
        int currentSelectCount = Cursor.Length;
        for (int i = 0; i < currentSelectCount; i++) {
            Select(Cursor.Last());
        }
        if (!active)
            return;
        
        for (int i = 0; i < capacity; i++) {
            Select(i);
        }
    }

    public virtual void Remove(int index) {
        if (!index.IsInRange(0, capacity))
            return;
        
        int refreshPage = GetRefreshPageAfterRemoved();
        storage.Remove(Selections[index]);
        SetPage(refreshPage);
    }

    public virtual int GetRefreshPageAfterRemoved() {
        bool isIsolated = ResultStorage.Count % capacity == 1;
        int refreshPage = Mathf.Max(0, isIsolated ? (Page - 1) : Page);
        return refreshPage;
    }

    public virtual void Replace(T newItem, int index) {
        if (!index.IsInRange(0, capacity))
            return;

        T oldItem = Selections[index];
        storage.Update(oldItem, newItem);
    }

    public virtual void ResetFilter(int defaultSelectPage = 0) {
        filter.SetFilterOptions();
        SetPage(defaultSelectPage);
    }

    public virtual void Filter(Func<T, bool> predicate, int defaultSelectPage = 0) {
        filter.SetFilterOptions(predicate);
        SetPage(defaultSelectPage);
    }

    public virtual void Filter(Func<T, int, bool> predicate, int defaultSelectPage = 0) {
        filter.SetFilterOptions(predicate);
        SetPage(defaultSelectPage);
    }

    public virtual void Sort<TKey>(Func<T, TKey> predicate, bool desc = true, int defaultSelectPage = 0) {
        if (predicate == null) {
            filter.SetSortingOptions();
        } else {
            filter.SetSortingOptions(desc, (x) => predicate.Invoke(x));
        }
        SetPage(defaultSelectPage);
    }
}

public enum PageMode {
    NewPage = 0,
    Indent = 1,
}