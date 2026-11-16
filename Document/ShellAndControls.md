# Shell and controls

Classes in `src/Template.WindowsApp/Shell` and custom controls in `src/Template.WindowsApp/Controls`.

## Shell

The side bar, the back and forward history, the command palette, the dialogs inside the window, and the window and settings management.

| Class | Kind | Description | Used in |
| --- | --- | --- | --- |
| NavigationMenu | Class | Side bar contents (Home, favorites, groups and items, Settings), recent pages, selection and unread counts. Restores the favorites and the recent pages | Main window, WindowManager (save and restore), Home, Settings |
| NavigationNode | Abstract class | Common part of items and groups (title, icon, emoji, unread count) | NavigationItem, NavigationGroup, Main window |
| NavigationItem | Class | Item for one page (view ID, selected state) | NavigationMenu, Main window, Home (recent pages), TaskbarManager (jump list) |
| NavigationGroup | Class | Group of items (children, expanded state, whether a child is selected) | NavigationMenu, Main window |
| NavigationHistory | Class | Back and forward history | Main window |
| ShellCommand | Class | One command in the palette (ID, title, category, icon, command, parameter) | 33 page view models, CommandPalette |
| IShellCommandProvider | Interface | Lets a page add its commands to the palette | 33 page view models, Main window |
| CommandPalette | Class | Palette state (open state, search text, candidates, selection) and ordering by recent use and count | Main window |
| CommandPaletteMove | Enum | Selection moves in the palette (previous, next, previous page, next page) | Main window |
| CommandCandidate | Class | One candidate row (command and the parts that match the search text) | CommandPalette, Main window |
| DialogHost | Class | Shows dialogs inside the window one at a time and restores the focus when one closes | Components/Dialog, Main window |
| IWindowManager, WindowManager | Interface and class | Reads and writes user.config (theme, start page, side bar, last page, recent commands and counts, grid layouts), restores the main window position, and shows, counts and activates other windows | App, Main window, Settings, Data grid, Resources & styles, Windows |
| AppTheme | Enum | App theme (Light, Dark, System) | WindowManager, Settings, Resources & styles |
| StartPage | Enum | Page at startup (Home, last page) | WindowManager, App, Settings |
| GridLayout, GridColumnLayout | Class | Saved grid layout (frozen columns, and the order, visibility and width of the columns), stored as JSON | WindowManager, Data grid |

CommandPaletteMove is at the top of `CommandPalette.cs`, and AppTheme, StartPage, GridLayout and GridColumnLayout are at the top of `IWindowManager.cs`.

## Controls

Custom controls in `Controls` (charts in `Controls/Charts`). The control templates are in `Themes/Generic.xaml`.

### Window

| Control | Base | Description | Used in |
| --- | --- | --- | --- |
| AppWindow | Window | Window with the custom title bar (caption buttons, snap layouts) and a lock that blocks closing, maximizing, moving and resizing | Main window, Sample window, Memory window, Error window |
| CaptionButton | Button | Minimize, maximize and close buttons of AppWindow | AppWindow |
| TitleBarPanel | Panel | Lays out the title bar contents | AppWindow |
| ContentDialog | ContentControl | Modal dialog inside the window | Dialogs (message, input, progress), Editable grid (bulk change) |
| InfoBar | Control | Message bar with a severity | Main window (notifications), Notifications |

### Input

| Control | Base | Description | Used in |
| --- | --- | --- | --- |
| AutoSuggestBox | Control | Text box with a suggestion list | Text |
| NumberBox | Control | Numeric input with spin buttons | Text, Web API, Editable grid (bulk change) |
| ToggleSwitch | ToggleButton | On and off switch | Choices, Live data, Logs, Sample window, Settings |
| TimePicker | Control | Hour and minute picker (TimePickerColumn, TimePickerRow) | Date & time |
| DropDownButton | ContentControl | Button that opens a menu | Buttons, Data grid, Messages |
| SplitButton | ContentControl | Button with a main action and a menu | Buttons, Logs |

### Layout

| Control | Base | Description | Used in |
| --- | --- | --- | --- |
| TabView | TabControl | Tabs with close buttons, reordering and width modes (TabViewItem, TabStripPanel) | Tabs |
| SettingsCard | ContentControl | Settings row with an icon, a header, a description and the content | Settings |
| SettingsPanel | StackPanel | Stack of settings cards | Settings |

### Display

| Control | Base | Description | Used in |
| --- | --- | --- | --- |
| EmojiIcon | FrameworkElement | Color emoji drawn with vector shapes (ColorEmoji, EmojiShape, EmojiLayer) | Side bar, Home, Hierarchy, Messages |
| BrushSwatch | Border | Color sample | Colors |
| RemovableTag | Control | Tag with a remove button | Custom controls |
| ProfileTile | UserControl | Profile card made as a UserControl | Custom controls |
| TemperatureTemplateSelector | DataTemplateSelector | Chooses a template by the temperature | Templates |
| XamlPresenter | ContentControl | Shows XAML text as live UI | Scratch pad |

### Charts

| Control | Base | Description | Used in |
| --- | --- | --- | --- |
| ChartBase | FrameworkElement | Base of the charts (values and labels) | LineChart, BarChart, DonutChart |
| LineChart | ChartBase | Line chart | Home, Charts, Live data, Grid visuals, Memory window |
| BarChart | ChartBase | Bar chart | Home, Charts |
| DonutChart | ChartBase | Donut chart with the total | Charts |
| Sparkline | FrameworkElement | Small line chart in a grid cell | Grid visuals |
| DataBar | FrameworkElement | Bar in a grid cell | Grid visuals |
