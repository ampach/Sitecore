using System;
using GraphQL.Types;
using Sitecore;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Data.Managers;
using Sitecore.Globalization;
using Sitecore.Mvc.Extensions;
using Sitecore.Services.GraphQL.Content;
using Sitecore.Services.GraphQL.Content.GraphTypes;
using Sitecore.Services.GraphQL.Schemas;
using Sitecore.Sites;

public class SelectSingleItemQuery : RootFieldType<ItemInterfaceGraphType, Item>, IContentSchemaRootFieldType
{
	public SelectSingleItemQuery()
		: base("selectSingleItemQuery", "Allows querying items from the content tree beneath specific site")
	{
		var queryArgumentArray = new QueryArgument[3];
		
		var languageArgument = new QueryArgument<StringGraphType>
		{
			Name = "language",
			Description = "The item language to request (defaults to the default language)"
		};
		queryArgumentArray[0] = languageArgument;
		var versionArgument = new QueryArgument<IntGraphType>
		{
			Name = "version",
			Description = "The item version to request (if not set, latest version is returned)"
		};
		queryArgumentArray[1] = versionArgument;
		
		var queryArgument = new QueryArgument<StringGraphType>
		{
			Name = "query",
			Description = "The Sitecore query"
		};
		queryArgumentArray[2] = queryArgument;
		this.Arguments = new QueryArguments(queryArgumentArray);
	}

	protected override Item Resolve(ResolveFieldContext context)
	{
        Item queryItem;
        var query = context.GetArgument("query", (string)null);
        if (!string.IsNullOrWhiteSpace(query))
        {
            queryItem = Sitecore.Context.Item.Axes.SelectSingleItem(query);
            if (queryItem == null)
                return null;
        }

        //Resolve language
        var languageName = context.GetArgument<string>("language", (string)null);
		Language lang = null;
		if (name != null && !Language.TryParse(languageName, out lang))
			throw new InvalidOperationException("Unable to parse requested language.");
		if (lang == null)
		{
			Language language = Context.Language;
			if ((object)language == null)
				language = LanguageManager.DefaultLanguage;
            lang = language;
		}

        //Resolve item version
		var version = context.GetArgument<int?>("version", new int?()) ?? -1;		
		
        //Resolve item
		Item obj;
		if (!IdHelper.TryResolveItem(this.Database, queryItem.ID, lang, new int?(version), out obj))
			return null;
		if (obj == null || obj.Versions.Count == 0)
			return null;
		return obj;
	}

	public Database Database { get; set; }
}