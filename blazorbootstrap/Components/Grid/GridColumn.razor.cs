﻿namespace BlazorBootstrap;

public partial class GridColumn<TItem> : BaseComponent
{
    #region Members

    private RenderFragment headerTemplate;

    private RenderFragment<TItem> cellTemplate;

    private FilterOperator filterOperator;

    private string filterValue;

    internal SortDirection currentSortDirection;

    internal SortDirection defaultSortDirection;

    #endregion Members

    #region Methods

    protected override async Task OnInitializedAsync()
    {
        ElementId = IdGenerator.Generate; // Required

        filterOperator = FilterOperator;
        filterValue = FilterValue;

        currentSortDirection = SortDirection;
        defaultSortDirection = SortDirection;

        if (IsDefaultSortColumn && SortDirection == SortDirection.None)
            currentSortDirection = SortDirection = SortDirection.Ascending;

        Parent.AddColumn(this);

        await base.OnInitializedAsync();
    }

    protected override void OnParametersSet()
    {
        SetDefaultFilter();
    }

    internal string GetPropertyTypeName()
    {
        if (string.IsNullOrWhiteSpace(this.PropertyName))
            return string.Empty;

        return typeof(TItem).GetProperty(this.PropertyName)?.PropertyType?.Name;
    }

    #region Filters

    internal FilterOperator GetFilterOperator() => this.filterOperator;

    internal string GetFilterValue() => this.filterValue;

    internal async Task OnFilterChangedAsync(FilterEventArgs args, GridColumn<TItem> column)
    {
        if (this.filterValue != args.Text || this.filterOperator != args.FilterOperator)
            await this.Parent.ResetPageNumberAsync(false);

        this.filterValue = args.Text;
        this.filterOperator = args.FilterOperator;
        await this.Parent.FilterChangedAsync();
    }

    internal void SetFilterOperator(FilterOperator filterOperator) => this.FilterOperator = this.filterOperator = filterOperator;

    internal void SetFilterValue(string filterValue) => this.FilterValue = this.filterValue = filterValue;

    #endregion Filters

    #region Sorting

    internal void SetDefaultFilter()
    {
        string propertyTypeName = this.GetPropertyTypeName();

        if (propertyTypeName == StringConstants.PropertyTypeNameInt16
            || propertyTypeName == StringConstants.PropertyTypeNameInt32
            || propertyTypeName == StringConstants.PropertyTypeNameInt64
            || propertyTypeName == StringConstants.PropertyTypeNameSingle // float
            || propertyTypeName == StringConstants.PropertyTypeNameDecimal
            || propertyTypeName == StringConstants.PropertyTypeNameDouble)
        {
            if (this.filterOperator == FilterOperator.None)
                this.FilterOperator = this.filterOperator = FilterOperator.Equals;
        }
        else if (propertyTypeName == StringConstants.PropertyTypeNameString
            || propertyTypeName == StringConstants.PropertyTypeNameChar)
        {
            if (this.filterOperator == FilterOperator.None)
                this.FilterOperator = this.filterOperator = FilterOperator.Contains;
        }
        else if (propertyTypeName == StringConstants.PropertyTypeNameDateOnly
            || propertyTypeName == StringConstants.PropertyTypeNameDateTime)
        {
            if (this.filterOperator == FilterOperator.None)
                this.FilterOperator = this.filterOperator = FilterOperator.Equals;
        }
        else if (propertyTypeName == StringConstants.PropertyTypeNameBoolean)
        {
            if (this.filterOperator == FilterOperator.None)
                this.FilterOperator = this.filterOperator = FilterOperator.Equals;
        }
    }

    internal bool CanSort() => Parent.AllowSorting && this.Sortable && this.SortKeySelector != null;

    internal IEnumerable<SortingItem<TItem>> GetSorting()
    {
        if (SortKeySelector == null && string.IsNullOrWhiteSpace(this.SortString))
            yield break;

        yield return new SortingItem<TItem>(this.SortString, this.SortKeySelector, this.currentSortDirection);
    }

    private async Task OnSortClickAsync()
    {
        // toggle the direction
        if (currentSortDirection == SortDirection.Ascending)
            currentSortDirection = SortDirection = SortDirection.Descending;
        else if (currentSortDirection == SortDirection.Descending)
            currentSortDirection = SortDirection = SortDirection.None;
        else if (currentSortDirection == SortDirection.None)
            currentSortDirection = SortDirection = SortDirection.Ascending;

        await Parent.SortingChangedAsync(this);
    }

    #endregion Sorting

    #endregion Methods

    #region Properties

    /// <inheritdoc/>
    protected override bool ShouldAutoGenerateId => true;

    [CascadingParameter] public Grid<TItem> Parent { get; set; }

    /// <summary>
    /// Gets or sets the table column header.
    /// </summary>
    [Parameter] public string HeaderText { get; set; }

    /// <summary>
    /// Gets or sets the property name.
    /// This is required when `AllowFiltering` is true.
    /// </summary>
    [Parameter] public string PropertyName { get; set; }

    /// <summary>
    /// Enable or disable the filter on a specific column.
    /// The filter is enabled or disabled based on the grid `AllowFiltering` parameter.
    /// </summary>
    [Parameter] public bool Filterable { get; set; } = true;

    /// <summary>
    /// Gets or sets the filter operator.
    /// </summary>
    [Parameter] public FilterOperator FilterOperator { get; set; }

    /// <summary>
    /// Gets or sets the filter value.
    /// </summary>
    [Parameter] public string FilterValue { get; set; }

    /// <summary>
    /// Gets or sets the StringComparison.
    /// </summary>
    [Parameter] public StringComparison StringComparison { get; set; } = StringComparison.OrdinalIgnoreCase;

    /// <summary>
    /// Enable or disable the sorting on a specific column.
    /// The sorting is enabled or disabled based on the `AllowSorting` parameter on the grid.
    /// </summary>
    [Parameter] public bool Sortable { get; set; } = true;

    /// <summary>
    /// Gets or sets the column sort string. 
    /// This value will be passed to the backend/API for sorting. 
    /// And this property is ignored for the client-side sorting.
    /// </summary>
    [Parameter] public string SortString { get; set; }

    /// <summary>
    /// Expression used for sorting.
    /// </summary>
    [Parameter] public Expression<Func<TItem, IComparable>> SortKeySelector { get; set; }

    /// <summary>
    /// Gets or sets the default sort direction of a column.
    /// </summary>
    [Parameter] public SortDirection SortDirection { get; set; } = SortDirection.None;

    /// <summary>
    /// Gets or sets the default sort column.
    /// </summary>
    [Parameter] public bool IsDefaultSortColumn { get; set; } = false;

    /// <summary>
    /// Specifies the content to be rendered inside the grid column.
    /// </summary>
    [Parameter] public RenderFragment<TItem> ChildContent { get; set; }

    internal RenderFragment HeaderTemplate
    {
        get
        {
            return headerTemplate ??= (builder =>
            {
                // th > span "title", span > i "icon"
                var seq = 0;
                builder.OpenElement(seq, "th");
                if (this.CanSort())
                {
                    seq++;
                    builder.AddAttribute(seq, "role", "button");
                    seq++;
                    builder.AddAttribute(seq, "onclick", async () => await OnSortClickAsync());
                }
                if (this.HeaderTextAlignment != Alignment.None)
                {
                    seq++;
                    builder.AddAttribute(seq, "class", BootstrapClassProvider.TextAlignment(this.HeaderTextAlignment));
                }
                seq++;
                builder.OpenElement(seq, "span");
                seq++;
                builder.AddAttribute(seq, "class", "me-2");
                seq++;
                builder.AddContent(seq, HeaderText);
                seq++;
                builder.CloseElement(); // close: span

                if (this.CanSort() && currentSortDirection != SortDirection.None)
                {
                    seq++;
                    builder.OpenElement(seq, "span");
                    seq++;
                    builder.OpenElement(seq, "i");
                    seq++;

                    var sortIcon = ""; // TODO: Add Parameter for this
                    if (currentSortDirection == SortDirection.Ascending)
                        sortIcon = "bi bi-sort-alpha-down";
                    else if (currentSortDirection == SortDirection.Descending)
                        sortIcon = "bi bi-sort-alpha-down-alt";

                    builder.AddAttribute(seq, "class", sortIcon);
                    builder.CloseElement(); // close: i
                    builder.CloseElement(); // close: span
                }

                builder.CloseElement(); // close: th
            });
        }
    }

    internal RenderFragment<TItem> CellTemplate
    {
        get
        {
            return cellTemplate ??= (rowData => builder =>
            {
                builder.OpenElement(100, "td");

                var classList = new List<string>();

                // text alignment
                if (this.TextAlignment != Alignment.None)
                    classList.Add(BootstrapClassProvider.TextAlignment(this.TextAlignment));

                // text nowrap
                if (this.TextNoWrap)
                    classList.Add(BootstrapClassProvider.TextNoWrap());

                // custom column class
                var columnClass = ColumnClass?.Invoke(rowData) ?? "";
                if(!string.IsNullOrWhiteSpace(columnClass))
                    classList.Add(columnClass);

                if (classList.Any())
                    builder.AddAttribute(101, "class", string.Join(" ", classList));

                builder.AddContent(102, ChildContent, rowData);

                builder.CloseElement();
            });
        }
    }

    /// <summary>
    /// Gets or sets the header text alignment.
    /// </summary>
    [Parameter] public Alignment HeaderTextAlignment { get; set; }

    /// <summary>
    /// Gets or sets the text alignment.
    /// </summary>
    [Parameter] public Alignment TextAlignment { get; set; }

    /// <summary>
    /// Gets or sets text nowrap.
    /// </summary>
    [Parameter] public bool TextNoWrap { get; set; }

    /// <summary>
    /// Gets or sets the filter textbox width in pixels.
    /// </summary>
    [Parameter] public int FilterTextboxWidth { get; set; }

    /// <summary>
    /// Gets or sets the column class.
    /// </summary>
    [Parameter] public Func<TItem, string>? ColumnClass { get; set; }

    #endregion Properties
}

