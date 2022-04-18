using System.Reflection.Metadata;

using ICSharpCode.ILSpy;
using ICSharpCode.ILSpy.Properties;
using ICSharpCode.ILSpyX;

namespace CustomSortAssemblies.Plugin;

[ExportMainMenuCommand(ParentMenuID = nameof(Resources._View), Header = MenuHeader, MenuIcon = "Images/Sort", MenuCategory = nameof(Resources.View))]
[ExportToolbarCommand(ToolTip = ToolbarTooltip, ToolbarIcon = "Images/Sort", ToolbarCategory = nameof(Resources.View))]
internal sealed class SortAssemblyListByNameThenVersionCommand : SimpleCommand, IComparer<LoadedAssembly>
{
    private const string MenuHeader = "Sort assembly list by name then by _version";
    private const string ToolbarTooltip = "Sort assembly list by name then by version";

    public override void Execute(object parameter)
    {
        using (MainWindow.Instance.AssemblyTreeView.LockUpdates())
            MainWindow.Instance.CurrentAssemblyList.Sort(this);
    }

    int IComparer<LoadedAssembly>.Compare(LoadedAssembly? x, LoadedAssembly? y)
    {
        if (x is null)
        {
            return -1;
        }

        if (y is null)
        {
            return 1;
        }

        int ret = string.Compare(x.ShortName, y.ShortName, StringComparison.CurrentCulture);

        if (ret != 0)
        {
            return ret;
        }

        if (x.IsLoaded && !x.HasLoadError && y.IsLoaded && !y.HasLoadError)
        {
            return CompareAssemblyVersion(x, y);
        }

        return 0;
    }

    private static int CompareAssemblyVersion(LoadedAssembly x, LoadedAssembly y)
    {
        MetadataReader? xMetadataReader = x.GetPEFileOrNull()?.Metadata;
        MetadataReader? yMetadataReader = y.GetPEFileOrNull()?.Metadata;

        if (xMetadataReader?.IsAssembly is true && yMetadataReader?.IsAssembly is true)
        {
            Version? xVersion = xMetadataReader.GetAssemblyDefinition().Version;
            Version? yVersion = yMetadataReader.GetAssemblyDefinition().Version;

            return Comparer<Version>.Default.Compare(xVersion, yVersion);
        }

        return 0;
    }
}
