using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Modulo9_ProvidedHostedWeb.Models;

namespace Modulo9_ProvidedHostedWeb.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult TotalPedidos()
        {
            //Cargas la conexión al sharepoint. Como puede ser con distintos sharepoints, se usa
            // el current dentro del provider
            var spContext = SharePointContextProvider.Current.GetSharePointContext(HttpContext);
            var clientContext = spContext.CreateUserClientContextForSPHost();
            using (clientContext)
            {
                if (clientContext != null)
                {
//Del cliente context recuperas "web" que es 
                    // lo que hay en la web a la que has conectado
                    Web web = clientContext.Web;
                    clientContext.Load(web);
                    clientContext.ExecuteQuery();
                    //Una vez cargada de la web obtienes las listas
                    ListCollection lists = web.Lists;
                    clientContext.Load<ListCollection>(lists);
                    clientContext.ExecuteQuery();

                    var pedidos = lists.GetByTitle("Pedidos");
                    var productos = lists.GetByTitle("Productos");
                    clientContext.Load(pedidos);
                    clientContext.Load(productos);
                    clientContext.ExecuteQuery();

                    //Se crea la caml query, aunque en este caso está vacía, pero podría
                    //levar las instrucciones (en xml) para filtrar por lo que fuese
                    CamlQuery pedidosQuery = new CamlQuery();
                    //Hay que pasar siempre una query, aunque esté vacía
                    ListItemCollection pedidosItems = pedidos.GetItems(pedidosQuery);
                    clientContext.Load(pedidosItems);
                    clientContext.ExecuteQuery();

                    var total = 0.0;
                    var clientes = new Dictionary<string, double>();

                    foreach (var item in pedidosItems)
                    {
                        //Indicas el valor del campo de búsqueda(lookup)
                        FieldLookupValue lookUp = item["Producto"] as FieldLookupValue;
                        //Recuperas el ID del campo lookup
                        int luID = lookUp.LookupId;
                        var uds = item["Unidades"];
                        //De la tabla productos, con el ID del campo de búsqueda, recuperas
                        //el resto de los datos
                        var prod = productos.GetItemById(luID);
                        clientContext.Load(prod);
                        clientContext.ExecuteQuery();

                        var precio = prod["Precio"];
                        var venta = (double) precio*(double) uds;
                        total += venta;

                        if (clientes.ContainsKey((item["Title"].ToString())))
                        {
                            clientes[item["Title"].ToString()] =
                                clientes[item["Title"].ToString()] +
                                venta;
                        }
                        else
                        {
                            clientes.Add(item["Title"].ToString(), venta);
                        }
                    }

                    var mc = total/clientes.Keys.Count;
                    var model = new Totales()
                    {
                        Numero = pedidosItems.Count,
                        MediaCliente = mc,
                        Total = total
                    };
                    return View(model);
                }
            }
            return null;
        }
    }
}
