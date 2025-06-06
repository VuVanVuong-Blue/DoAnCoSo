using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.IO;
using System.Threading.Tasks;

namespace System_Music.Extensions
{
    public static class ControllerExtensions
    {
        public static async Task<string> RenderViewAsync<TModel>(
            this ControllerBase controller,
            string viewName,
            TModel model,
            ViewDataDictionary viewData,
            ITempDataDictionary tempData,
            bool partial = false)
        {
            var viewEngine = controller.HttpContext.RequestServices.GetService(typeof(ICompositeViewEngine)) as ICompositeViewEngine;
            if (viewEngine == null)
            {
                throw new InvalidOperationException("ICompositeViewEngine not available.");
            }

            var viewResult = viewEngine.FindView(controller.ControllerContext, viewName, !partial);

            if (!viewResult.Success)
            {
                var searchedLocations = viewResult.SearchedLocations != null
                    ? string.Join(", ", viewResult.SearchedLocations)
                    : "No locations searched.";
                throw new FileNotFoundException($"View {viewName} not found. Searched locations: {searchedLocations}");
            }

            var viewContext = new ViewContext(
                controller.ControllerContext,
                viewResult.View,
                viewData ?? new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary()),
                tempData ?? new TempDataDictionary(controller.HttpContext, new MockTempDataProvider()),
                new StringWriter(),
                new HtmlHelperOptions()
            );

            viewContext.ViewData.Model = model;

            using (var writer = new StringWriter())
            {
                await viewResult.View.RenderAsync(viewContext);
                return writer.ToString();
            }
        }
    }

    public class MockTempDataProvider : ITempDataProvider
    {
        public IDictionary<string, object> LoadTempData(HttpContext context)
        {
            return new Dictionary<string, object>();
        }

        public void SaveTempData(HttpContext context, IDictionary<string, object> values)
        {
        }
    }
}