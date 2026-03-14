using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.Annotations; 
using SpravaOsobnichFinanci.Models;
using SpravaOsobnichFinanci.Commands;
using SpravaOsobnichFinanci.Controls;

namespace SpravaOsobnichFinanci.ViewModels
{
    /// <summary>
    /// Sdružuje a zajišťuje vizualizační logiku pro úvodní obrazovku (Dashboard).
    /// Počítá měsíční souhrny, vykresluje stav widgetů a mapuje data z DB do struktury grafické knihovny OxyPlot.
    /// </summary>
    internal class DashboardViewModel : BaseViewModel
    {
        private readonly DatabaseContext _dbContext;
        private DateTime _selectedMonth;

        // --- Stavové proměnné (Data pro Binding) ---
        private decimal _totalBalance;
        private decimal _monthlyIncome;
        private decimal _monthlyExpense;
        private decimal _monthlyBalance;
        private decimal _endOfMonthBalance;
        private decimal _maxCashFlow;
        private bool _isAddWidgetButtonVisible;
        private bool _isAddWidgetPopupOpen;

        // --- Příkazy (Interakce uživatele) ---
        private readonly ICommand _previousMonthCommand;
        private readonly ICommand _nextMonthCommand;
        private readonly ICommand _hideWidgetCommand;
        private readonly ICommand _showWidgetCommand;
        private readonly ICommand _toggleAddWidgetPopupCommand;

        // --- Modely vizualizací (OxyPlot) ---
        // Instancování PlotModelů probíhá záměrně pouze jednou. Zabraňuje to "problikávání" a mizení grafů v XAMLu
        // při změně měsíce. Při aktualizaci dat se pouze čistí a plní vnitřní kolekce Series
        private readonly PlotModel _balanceChartModel = new PlotModel { Background = OxyColors.Transparent, PlotAreaBorderThickness = new OxyThickness(0), IsLegendVisible = false };
        private readonly PlotModel _incomeChartModel = new PlotModel { Background = OxyColors.Transparent, IsLegendVisible = false };
        private readonly PlotModel _expenseChartModel = new PlotModel { Background = OxyColors.Transparent, IsLegendVisible = false };

        // Kolekce pro zobrazení legendy vedle kruhových grafů
        // Obsahují pouze textové informace, které se mapují do UI přes XAML
        private readonly ObservableCollection<ChartLegendItem> _incomeLegend;
        private readonly ObservableCollection<ChartLegendItem> _expenseLegend;
        private readonly PlotController _hoverController;
        private readonly PlotController _lineChartController;
        private LineAnnotation? _crosshairLine;

        // --- Veřejné Properties pro UI Binding (PascalCase) ---
        public DateTime SelectedMonth
        {
            get => _selectedMonth;
            set 
            { 
                if (SetProperty(ref _selectedMonth, value))
                {
                    OnPropertyChanged(nameof(SelectedMonthText));
                    RefreshDashboard();
                }
            }
        }

        // Formátování zobrazení měsíce pro nadpis na dashboardu (např. "Leden 2024")
        public string SelectedMonthText => _selectedMonth.ToString("MMMM yyyy", new System.Globalization.CultureInfo("cs-CZ"));

        // Příkazy pro přepínání mezi měsíci
        public ICommand PreviousMonthCommand => _previousMonthCommand;
        public ICommand NextMonthCommand => _nextMonthCommand;

        // Finanční ukazatele pro zobrazení v textových widgetech
        public decimal TotalBalance
        {
            get => _totalBalance;
            set => SetProperty(ref _totalBalance, value);
        }

        /// Měsíční souhrny pro zobrazení v jednotlivých widgetech
        public decimal MonthlyIncome
        {
            get => _monthlyIncome;
            set => SetProperty(ref _monthlyIncome, value);
        }

        // Měsíční souhrny pro zobrazení v jednotlivých widgetech
        public decimal MonthlyExpense
        {
            get => _monthlyExpense;
            set => SetProperty(ref _monthlyExpense, value);
        }

        // Měsíční souhrny pro zobrazení v jednotlivých widgetech
        public decimal MonthlyBalance
        {
            get => _monthlyBalance;
            set => SetProperty(ref _monthlyBalance, value);
        }

        // Zůstatek k poslednímu dni zvoleného měsíce (nebo k dnešnímu dni, pokud je zvolen aktuální měsíc)
        // Slouží pro zobrazení v grafu a textovém widgetu
        public decimal EndOfMonthBalance
        {
            get => _endOfMonthBalance;
            set => SetProperty(ref _endOfMonthBalance, value);
        }

        // Parametr pro vizuální progres bary v cashflow widgetu
        // Dynamicky se nastavuje na nejvyšší hodnotu mezi měsíčním příjmem a výdajem, aby se zajistilo správné škálování
        public decimal MaxCashFlow
        {
            get => _maxCashFlow;
            set => SetProperty(ref _maxCashFlow, value);
        }

        // --- Řízení viditelnosti bloků na panelu (Widgety) ---
        public bool IsBalanceVisible
        {
            get => _dbContext.Settings.IsBalanceWidgetVisible;
            set 
            { 
                _dbContext.Settings.IsBalanceWidgetVisible = value; 
                OnPropertyChanged(); 
                UpdateAddWidgetButtonVisibility();
            }
        }

        // Viditelnost pro cashflow widget se řídí stejnou logikou jako pro zůstatek, příjmy a výdaje
        // Uživatel může skrýt všechny tyto bloky, ale pak se mu zobrazí tlačítko pro jejich opětovné přidání
        public bool IsCashFlowVisible
        {
            get => _dbContext.Settings.IsCashFlowWidgetVisible;
            set 
            { 
                _dbContext.Settings.IsCashFlowWidgetVisible = value; 
                OnPropertyChanged(); 
                UpdateAddWidgetButtonVisibility();
            }
        }

        // Viditelnost pro blok příjmů
        public bool IsIncomeVisible
        {
            get => _dbContext.Settings.IsIncomeWidgetVisible;
            set 
            { 
                _dbContext.Settings.IsIncomeWidgetVisible = value; 
                OnPropertyChanged(); 
                UpdateAddWidgetButtonVisibility();
            }
        }

        // Viditelnost pro blok výdajů
        public bool IsExpenseVisible
        {
            get => _dbContext.Settings.IsExpenseWidgetVisible;
            set 
            { 
                _dbContext.Settings.IsExpenseWidgetVisible = value; 
                OnPropertyChanged(); 
                UpdateAddWidgetButtonVisibility();
            }
        }

        // Tlačítko pro přidání widgetů se zobrazuje pouze v případě, že alespoň jeden z hlavních bloků (Zůstatek, CashFlow, Příjmy, Výdaje) není viditelný
        public bool IsAddWidgetButtonVisible
        {
            get => _isAddWidgetButtonVisible;
            set => SetProperty(ref _isAddWidgetButtonVisible, value);
        }

        // Stav pro zobrazení popupu s možností přidání widgetů
        // Ovládá se přes příkazy pro zobrazení/skrývání jednotlivých widgetů a také přes tlačítko pro otevření/zavření popupu
        public bool IsAddWidgetPopupOpen
        {
            get => _isAddWidgetPopupOpen;
            set => SetProperty(ref _isAddWidgetPopupOpen, value);
        }

        // Příkazy pro skrývání a zobrazování jednotlivých widgetů na dashboardu. Parametr určuje, o který widget se jedná
        public ICommand HideWidgetCommand => _hideWidgetCommand;
        public ICommand ShowWidgetCommand => _showWidgetCommand;
        public ICommand ToggleAddWidgetPopupCommand => _toggleAddWidgetPopupCommand;

        // Modely grafů pro zobrazení v XAMLu. Jsou inicializovány pouze jednou, aby se zabránilo blikání a znovuvykreslování celého grafu při každé změně měsíce
        // Při aktualizaci dat se pouze mění obsah vnitřních kolekcí Series
        public PlotModel BalanceChartModel => _balanceChartModel; 
        public PlotModel IncomeChartModel => _incomeChartModel; 
        public PlotModel ExpenseChartModel => _expenseChartModel;

        // Kolekce pro zobrazení legendy vedle kruhových grafů
        // Obsahují pouze textové informace, které se mapují do UI přes XAML
        public ObservableCollection<ChartLegendItem> IncomeLegend => _incomeLegend;
        public ObservableCollection<ChartLegendItem> ExpenseLegend => _expenseLegend;
        public PlotController HoverController => _hoverController;
        public PlotController LineChartController => _lineChartController;

        /// <summary>
        /// Konstruktor hlavního ViewModelu pro Dashboard. Inicializuje všechny potřebné kolekce, modely grafů a příkazy.
        /// </summary>
        /// <param name="dbContext">Instance databázového kontextu pro načítání a manipulaci s daty transakcí, kategorií a nastavení.</param>
        public DashboardViewModel(DatabaseContext dbContext)
        {
            _dbContext = dbContext;
            
            _incomeLegend = new ObservableCollection<ChartLegendItem>();
            _expenseLegend = new ObservableCollection<ChartLegendItem>();
            
            // Konfigurace ovladačů (Controllers) pro modifikaci defaultního chování myši nad grafem
            _hoverController = new PlotController();
            _hoverController.BindMouseEnter(PlotCommands.HoverSnapTrack);
            _hoverController.UnbindMouseDown(OxyMouseButton.Left); // Vypnutí nepříjemného přibližování
            
            _lineChartController = new PlotController();
            _lineChartController.BindMouseEnter(PlotCommands.HoverSnapTrack);
            _lineChartController.UnbindMouseDown(OxyMouseButton.Left);
            
            _hideWidgetCommand = new RelayCommand<string>(HideWidget);
            _showWidgetCommand = new RelayCommand<string>(ShowWidget);
            _toggleAddWidgetPopupCommand = new RelayCommand(ToggleAddWidgetPopup);
            
            _previousMonthCommand = new RelayCommand(() => SelectedMonth = SelectedMonth.AddMonths(-1));
            _nextMonthCommand = new RelayCommand(() => SelectedMonth = SelectedMonth.AddMonths(1));

            // Zobrazení se inicializuje s aktuálním kalendářním měsícem
            _selectedMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            
            UpdateAddWidgetButtonVisibility();
            RefreshDashboard(); 
        }

        // Přepíná stav zobrazení popupu pro přidávání widgetů. Ovládá se přes tlačítko "+" v UI.
        private void ToggleAddWidgetPopup() => IsAddWidgetPopupOpen = !IsAddWidgetPopupOpen;

        /// <summary>
        /// Skrývá zvolený widget na dashboardu. Volá se z UI s parametrem určujícím, který widget skrýt.
        /// </summary>
        /// <param name="widgetName">Název widgetu, který se má skrýt (např. "Balance", "CashFlow", "Income", "Expense").</param>
        private void HideWidget(string? widgetName) 
        {
            switch (widgetName)
            {
                case "Balance": IsBalanceVisible = false; break;
                case "CashFlow": IsCashFlowVisible = false; break;
                case "Income": IsIncomeVisible = false; break;
                case "Expense": IsExpenseVisible = false; break;
            }
        }

        /// <summary>
        /// Zobrazuje zvolený widget na dashboardu. Volá se z UI s parametrem určujícím, který widget zobrazit.
        /// </summary>
        /// <param name="widgetName">Název widgetu, který se má zobrazit (např. "Balance", "CashFlow", "Income", "Expense").</param>
        private void ShowWidget(string? widgetName) 
        {
            switch (widgetName)
            {
                case "Balance": IsBalanceVisible = true; break;
                case "CashFlow": IsCashFlowVisible = true; break;
                case "Income": IsIncomeVisible = true; break;
                case "Expense": IsExpenseVisible = true; break;
            }
            IsAddWidgetPopupOpen = false;
        }

        /// <summary>
        /// Aktualizuje viditelnost tlačítka pro přidání widgetů na základě aktuálního stavu zobrazení hlavních bloků (Zůstatek, CashFlow, Příjmy, Výdaje).
        /// </summary>
        private void UpdateAddWidgetButtonVisibility()
        {
            // Tlačítko pro přidání widgetů se ukáže jen ve chvíli, kdy aspoň jeden blok chybí na obrazovce
            IsAddWidgetButtonVisible = !IsBalanceVisible || !IsCashFlowVisible || !IsIncomeVisible || !IsExpenseVisible;
            if (!IsAddWidgetButtonVisible) IsAddWidgetPopupOpen = false;
        }

        /// <summary>
        /// Přepočítá agragace a kompletně znovuvykreslí všechny grafy na obrazovce.
        /// Volá se primárně po změně filtrovaného obodobí.
        /// </summary>
        private void RefreshDashboard()
        {
            CalculateSummaries();
            GenerateChartsData();

            OnPropertyChanged(nameof(BalanceChartModel));
            OnPropertyChanged(nameof(IncomeChartModel));
            OnPropertyChanged(nameof(ExpenseChartModel));
        }

        /// <summary>
        /// Transformuje surová databázová do čistých finančních ukazatelů pro zobrazení v textových Widgetech.
        /// </summary>
        private void CalculateSummaries()
        {
            decimal totalIncome = _dbContext.Transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
            decimal totalExpense = _dbContext.Transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);
            TotalBalance = totalIncome - totalExpense;

            var selectedMo = SelectedMonth.Month;
            var selectedYr = SelectedMonth.Year;

            MonthlyIncome = _dbContext.Transactions
                .Where(t => t.Type == TransactionType.Income && t.Date.Month == selectedMo && t.Date.Year == selectedYr)
                .Sum(t => t.Amount);

            MonthlyExpense = _dbContext.Transactions
                .Where(t => t.Type == TransactionType.Expense && t.Date.Month == selectedMo && t.Date.Year == selectedYr)
                .Sum(t => t.Amount);

            MonthlyBalance = MonthlyIncome - MonthlyExpense;

            // Parametr pro vizuální progres bary (vyhýbá se dělení 0 v XAMLu)
            MaxCashFlow = Math.Max(Math.Max(MonthlyIncome, MonthlyExpense), 1m);

            var lastDayOfSelectedMonth = new DateTime(selectedYr, selectedMo, 1).AddMonths(1).AddDays(-1);
            var stopDateForBalance = (selectedMo == DateTime.Now.Month && selectedYr == DateTime.Now.Year) ? DateTime.Now.Date : lastDayOfSelectedMonth;

            EndOfMonthBalance = _dbContext.Transactions
                .Where(t => t.Date.Date <= stopDateForBalance)
                .Sum(t => t.Type == TransactionType.Income ? t.Amount : -t.Amount);
        }

        /// <summary>
        /// Obsluhuje plnění lineárního grafu (vývoj zůstatku) pro prostřední sekci nástěnky.
        /// </summary>
        private void GenerateChartsData()
        {
            _balanceChartModel.Axes.Clear();
            _balanceChartModel.Series.Clear();
            _balanceChartModel.Annotations.Clear();
            
            var firstDayOfSelectedMonth = new DateTime(SelectedMonth.Year, SelectedMonth.Month, 1);
            var lastDayOfSelectedMonth = firstDayOfSelectedMonth.AddMonths(1).AddDays(-1);

            var datesInMonth = _dbContext.Transactions
                .Where(t => t.Date.Month == SelectedMonth.Month && t.Date.Year == SelectedMonth.Year)
                .Select(t => t.Date)
                .ToList();

            var maxDataDateInMonth = datesInMonth.Any() ? datesInMonth.Max().Date : DateTime.MinValue.Date;
            DateTime stopDrawingDate;
            
            // Pokud jsme v současném měsíci, krivka grafu zůstatku se zařízne v dnešním dni a nepokračuje dál
            if (SelectedMonth.Month == DateTime.Now.Month && SelectedMonth.Year == DateTime.Now.Year)
            {
                stopDrawingDate = maxDataDateInMonth > DateTime.Now.Date ? maxDataDateInMonth : DateTime.Now.Date;
            }
            else
            {
                stopDrawingDate = lastDayOfSelectedMonth;
            }

            var dateAxis = new DateTimeAxis
            {
                Position = AxisPosition.Bottom,
                StringFormat = "dd.MM.",
                AxislineStyle = LineStyle.None,
                MajorTickSize = 4,
                TickStyle = TickStyle.Outside,
                MinorTickSize = 0,
                Minimum = DateTimeAxis.ToDouble(firstDayOfSelectedMonth),
                Maximum = DateTimeAxis.ToDouble(lastDayOfSelectedMonth),
                IntervalType = DateTimeIntervalType.Days,
                MajorStep = 5,
                IsZoomEnabled = false,
                IsPanEnabled = false
            };
            _balanceChartModel.Axes.Add(dateAxis);

            var valueAxis = new LinearAxis
            {
                Position = AxisPosition.Left,
                StringFormat = "N0",
                MajorGridlineStyle = LineStyle.Dash, 
                MajorGridlineColor = OxyColor.Parse("#A0A0A0"), 
                AxislineStyle = LineStyle.None,
                MajorTickSize = 4,
                TickStyle = TickStyle.Outside,
                MinorTickSize = 0,
                ExtraGridlines = new double[] { 0 },
                ExtraGridlineStyle = LineStyle.Dash,
                ExtraGridlineColor = OxyColor.Parse("#A0A0A0"),
                MinimumPadding = 0.1,
                MaximumPadding = 0.1,
                IsZoomEnabled = false,
                IsPanEnabled = false
            };
            _balanceChartModel.Axes.Add(valueAxis);

            // Vertikální stínová vodící čára, která reaguje na prejíždění kurzoru
            _crosshairLine = new LineAnnotation
            {
                Type = LineAnnotationType.Vertical,
                Color = OxyColor.Parse("#CCCCCC"), 
                StrokeThickness = 1,
                LineStyle = LineStyle.Dash,
                X = double.NaN, 
                Layer = AnnotationLayer.BelowSeries 
            };
            _balanceChartModel.Annotations.Add(_crosshairLine);

            var lineSeries = new VerticalTrackingStairStepSeries
            {
                Title = "Zůstatek",
                Color = OxyColor.Parse("#2196F3"),
                StrokeThickness = 2,
                MarkerType = MarkerType.None,
                TrackerFormatString = "Datum: {2:dd.MM.yyyy}\nČástka: {4:N2} " + (_dbContext.Settings?.CurrencySymbol ?? "Kč")
            };

            // Počáteční offset: Všechny dříve zaznamenané transakce (do začátku daného měsíce)
            decimal runningBalance = _dbContext.Transactions
                .Where(t => t.Date < firstDayOfSelectedMonth)
                .Sum(t => t.Type == TransactionType.Income ? t.Amount : -t.Amount);

            var start = firstDayOfSelectedMonth.Date;
            var end = stopDrawingDate.Date;

            if (start == end) end = end.AddDays(1);

            lineSeries.Points.Add(new DataPoint(DateTimeAxis.ToDouble(start), (double)runningBalance));

            for (DateTime date = start; date <= end; date = date.AddDays(1))
            {
                decimal dailyNet = _dbContext.Transactions
                    .Where(t => t.Date.Date == date)
                    .Sum(t => t.Type == TransactionType.Income ? t.Amount : -t.Amount);

                runningBalance += dailyNet;

                if (dailyNet != 0 || date == end)
                {
                    lineSeries.Points.Add(new DataPoint(DateTimeAxis.ToDouble(date), (double)runningBalance));
                }
            }

            if (lineSeries.Points.Count == 1)
            {
                lineSeries.Points.Add(new DataPoint(DateTimeAxis.ToDouble(end), (double)runningBalance));
            }

            _balanceChartModel.Series.Add(lineSeries);
            
            // Obsluha pro vodící kříž (CrossHair)
#pragma warning disable CS0618
            _balanceChartModel.TrackerChanged += (s, e) =>
            {
                if (e.HitResult != null && _crosshairLine != null)
                {
                    if (e.HitResult.Item is DataPoint dp)
                    {
                        _crosshairLine.X = dp.X;
                    }
                    _balanceChartModel.InvalidatePlot(false);
                }
                else if (e.HitResult == null && _crosshairLine != null)
                {
                    _crosshairLine.X = double.NaN;
                    _balanceChartModel.InvalidatePlot(false);
                }
            };

            _balanceChartModel.InvalidatePlot(true); 

            // Generování vedlejších kruhových grafů poměrů výdajů
            UpdatePieChart(TransactionType.Income, _incomeChartModel, _incomeLegend);
            UpdatePieChart(TransactionType.Expense, _expenseChartModel, _expenseLegend);
        }

        /// <summary>
        /// Vyplní jeden kruhový koláčový graf na základě součtu kategorií zastupujících daný typ plnění (Příjmy / Výdaje).
        /// </summary>
        private void UpdatePieChart(TransactionType type, PlotModel model, ObservableCollection<ChartLegendItem> targetLegendList)
        {
            model.Series.Clear();

            var transactionsInMonth = _dbContext.Transactions
                .Where(t => t.Type == type && t.Date.Month == SelectedMonth.Month && t.Date.Year == SelectedMonth.Year)
                .ToList();

            var totalAmount = transactionsInMonth.Sum(t => t.Amount);
            targetLegendList.Clear();

            if (totalAmount == 0)
            {
                // Fallback do neutrálního prázdného kolečka
                var emptySeries = new PieSeries
                {
                    StrokeThickness = 0,
                    InsideLabelPosition = 0.5,
                    InsideLabelFormat = "",
                    OutsideLabelFormat = "",
                    TickHorizontalLength = 0,
                    TickRadialLength = 0,
                    Diameter = 0.95,
                    InnerDiameter = 0.6,
                    TrackerFormatString = "Nejsou k dispozici žádná data"
                };
                emptySeries.Slices.Add(new PieSlice("Nejsou k dispozici žádná data", 1) { Fill = OxyColor.Parse("#E0E0E0") });
                model.Series.Add(emptySeries);
                model.InvalidatePlot(true);
                return;
            }

            var grouped = transactionsInMonth
                .GroupBy(t => t.CategoryId)
                .Select(g => new
                {
                    CategoryId = g.Key,
                    Total = g.Sum(t => t.Amount)
                })
                .OrderByDescending(g => g.Total)
                .ToList();

            var pieSeries = new PieSeries
            {
                StrokeThickness = 2,
                InsideLabelPosition = 0.5,
                InsideLabelFormat = "", 
                OutsideLabelFormat = "", 
                TickHorizontalLength = 0,
                TickRadialLength = 0,
                Diameter = 0.95,
                InnerDiameter = 0.6,
                TrackerFormatString = "{1}: {2:N2} " + (_dbContext.Settings?.CurrencySymbol ?? "Kč") + " ({3:P1})" 
            };

            foreach (var group in grouped)
            {
                var category = _dbContext.Categories.FirstOrDefault(c => c.Id == group.CategoryId) 
                               ?? new Category { Name = "Neznámá", ColorHex = "#999999", IconKey="Help" };
                
                // Formátování pro eliminaci nápisu 0% u velmi malých transakcí 
                double percentage = (double)(group.Total / totalAmount) * 100;
                string formatString = percentage < 1 ? "<1" : percentage.ToString("F0");

                pieSeries.Slices.Add(new PieSlice(category.Name, (double)group.Total)
                {
                    Fill = OxyColor.Parse(category.ColorHex)
                });

                targetLegendList.Add(new ChartLegendItem
                {
                    CategoryName = category.Name,
                    ColorHex = category.ColorHex,
                    Icon = category.IconKey,
                    Amount = group.Total,
                    PercentageText = $"{formatString}%"
                });
            }

            model.Series.Add(pieSeries);
            
            // Dummy logika zachytávající uživatelské proklikávání dílků grafu
#pragma warning disable CS0618
            model.TrackerChanged += (s, e) =>
            {
                var ps = model.Series.OfType<PieSeries>().FirstOrDefault();
                if (ps != null)
                {
                    if (e.HitResult != null && e.HitResult.Item is PieSlice slice)
                    {
                        foreach (var targetSlice in ps.Slices)
                        {
                            if (targetSlice == slice) continue;
                        }
                    }
                    model.InvalidatePlot(false);
                }
            };

            model.InvalidatePlot(true);
        }
    }
}
