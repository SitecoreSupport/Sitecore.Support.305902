
namespace Sitecore.Support.XA.Feature.CreativeExchange.Pipelines.Import.Import
{
  using Sitecore.Configuration;
  using Sitecore.Data;
  using Sitecore.Data.Items;
  using Sitecore.Pipelines;
  using Sitecore.XA.Feature.CreativeExchange.Pipelines.Import.AssetProcessing;
  using Sitecore.XA.Feature.CreativeExchange.Pipelines.Import.Import;
  using Sitecore.XA.Feature.CreativeExchange.Storage;
  using System;
  using System.Collections.Generic;
  using Microsoft.Practices.EnterpriseLibrary.Common.Utility;
  using System.Linq;

  public class AssetEnumeration : ImportBaseProcessor
  {
    public override void Process(ImportArgs args)
    {
      IEnumerable<IImportEntry> sorted = args.CreativeExchangeStorage.Entries.OrderBy(x => x.Path);
      (from e in sorted
       where IsAsset(e.Path)
       select e).ForEach(delegate (IImportEntry s) { ProcessAsset(s, args); });
    }

    protected virtual void ProcessAsset(IImportEntry entry, ImportArgs importArgs)
    {
      AssetProcessingArgs assetProcessingArgs = new AssetProcessingArgs
      {
        ImportEntry = entry,
        Database = importArgs.ImportContext.Database,
        Messages = importArgs.Messages,
        CustomImportData = importArgs.CustomData
      };
      CorePipeline.Run("ceImport.assetProcessing", assetProcessingArgs);
      if ((IsCssAsset(entry.Path) || IsJsAsset(entry.Path)) && assetProcessingArgs.ResolvedItem != null)
      {
        ID parentID = assetProcessingArgs.ResolvedItem.ParentID;
        if (!importArgs.ThemeAssets.ContainsKey(parentID))
        {
          importArgs.ThemeAssets.Add(parentID, new HashSet<Item>());
        }

        importArgs.ThemeAssets[parentID].Add(assetProcessingArgs.ResolvedItem);
      }
    }

    private bool IsAsset(string path)
    {
      return path.TrimStart('/').StartsWith(Settings.Media.MediaLinkPrefix, StringComparison.Ordinal);
    }

    private bool IsCssAsset(string path)
    {
      if (IsAsset(path))
      {
        return path.EndsWith(".css", StringComparison.OrdinalIgnoreCase);
      }

      return false;
    }

    private bool IsJsAsset(string path)
    {
      if (IsAsset(path))
      {
        return path.EndsWith(".js", StringComparison.OrdinalIgnoreCase);
      }

      return false;
    }
  }
}