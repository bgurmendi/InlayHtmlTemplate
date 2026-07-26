# Titulo

## Subtitulo

Quiero entender cómo podría ser el.modelo de una pantalla de web de C# con htmx. Quiero definir en la pantalla una tabla de facts. Para un cubo y relacionarlo con información descriptiva. Tambien una línea de tiempo.

Uso entity framework así que las relaciones pueden estar ahí definidas.

Entonces en un sitio quiero definir la consulta base o varias de ellas.

En otro lugar decir que quiero usar tal dato y mostrar algunas gráficas y datos.

Al.mostrar los datos, se podrá filtrar y se aplica a todos los gráficos de la pantalla. Se podrá hacer drilldown en algunos o ir a los datos concretos.

No tiene que ser 100% perfecto, si no cubrir una buena parte de capacidad de informe.

Architected star schema analytics framework integrating Entity Framework, htmx, and dynamic filtering

Architected star schema analytics framework integrating Entity Framework, htmx, and dynamic filtering

Buena arquitectura para esto. Te planteo un modelo por capas, pensado para EF Core + htmx, que cubre lo que pides sin sobre-ingeniería.

### 1. Modelo de datos (EF) — esquema estrella

La "tabla de facts" es literalmente eso: una tabla de hechos (numéricos, medibles) con FKs a dimensiones descriptivas. EF Core resuelve las relaciones solo con navigation properties.

csharp

```csharp
public class FactVenta
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; }
    public decimal Importe { get; set; }
    public int Cantidad { get; set; }

    public int ProductoId { get; set; }
    public DimProducto Producto { get; set; }

    public int RegionId { get; set; }
    public DimRegion Region { get; set; }

    public int ClienteId { get; set; }
    public DimCliente Cliente { get; set; }
}

public class DimProducto
{
    public int Id { get; set; }
    public string Nombre { get; set; }
    public string Categoria { get; set; }
}
```

La línea de tiempo no es una entidad distinta: es la misma tabla de facts agrupada por `Fecha` (día/semana/mes). No necesitas modelar nada especial para eso, solo una forma de agrupar en la consulta.

### 2. Capa de "cubo": medidas y dimensiones

Define, sobre el fact, qué se puede **medir** y por qué se puede **agrupar** (dimensiones). Con expression trees queda genérico:

csharp

```csharp
public class CuboDefinicion<TFact>
{
    public Dictionary<string, Expression<Func<IGrouping<object, TFact>, decimal>>> Medidas { get; } = new();
    public Dictionary<string, Expression<Func<TFact, object>>> Dimensiones { get; } = new();
}
```

En la práctica, para no complicarte, puedes empezar con algo más simple: un enum o diccionario de "medidas" (`SumaImporte`, `Cantidad`, `NumVentas`) y otro de "dimensiones" (`Producto`, `Región`, `Categoría`, `Mes`), cada una resuelta con un `switch` a una expresión LINQ. Es menos elegante pero mucho más fácil de mantener.

### 3. Consultas base (un sitio central)

Aquí defines las `IQueryable` reutilizables, ya con los joins resueltos por EF (navigation properties):

csharp

```csharp
public class VentasQueries
{
    private readonly AppDbContext _db;
    public VentasQueries(AppDbContext db) => _db = db;

    public IQueryable<FactVenta> Base() =>
        _db.Facts.Include(f => f.Producto).Include(f => f.Region).Include(f => f.Cliente);

    public IQueryable<FactVenta> PorRegion() => Base(); // mismo base, distinto agrupado luego
    public IQueryable<FactVenta> PorProducto() => Base();
}
```

La idea: **una sola consulta base por dominio**, y las distintas vistas la reutilizan aplicando distinto `GroupBy`/`Where` encima. Si necesitas varias porque difieren en joins, defines varias (`VentasConDevoluciones()`, etc.), pero todas centralizadas aquí — no dispersas en cada controlador.

### 4. Filtro compartido (aplica a toda la pantalla)

csharp

```csharp
public class FiltroDashboard
{
    public DateTime? Desde { get; set; }
    public DateTime? Hasta { get; set; }
    public int? RegionId { get; set; }
    public int? CategoriaId { get; set; }
}

public static IQueryable<FactVenta> AplicarFiltro(this IQueryable<FactVenta> q, FiltroDashboard f)
{
    if (f.Desde != null) q = q.Where(x => x.Fecha >= f.Desde);
    if (f.Hasta != null) q = q.Where(x => x.Fecha <= f.Hasta);
    if (f.RegionId != null) q = q.Where(x => x.RegionId == f.RegionId);
    if (f.CategoriaId != null) q = q.Where(x => x.Producto.CategoriaId == f.CategoriaId);
    return q;
}
```

Este `FiltroDashboard` viaja como querystring/form (htmx lo manda con `hx-include`) y se aplica **antes** de cada widget calcular sus datos. Así un solo filtro afecta a todos los gráficos.

### 5. Definición de la pantalla (qué widgets mostrar)

Aquí es donde "dices qué dato usar y qué gráfico mostrar":

csharp

```csharp
public class WidgetDefinicion
{
    public string Id { get; set; }
    public string Titulo { get; set; }
    public TipoGrafico Tipo { get; set; } // Barras, Linea, Pie, Timeline, Tabla
    public string Dimension { get; set; } // "Producto", "Region", "Mes"
    public string Medida { get; set; }    // "SumaImporte", "Cantidad"
    public bool PermiteDrilldown { get; set; }
}

public class DashboardVentas
{
    public List<WidgetDefinicion> Widgets => new()
    {
        new() { Id="w1", Titulo="Ventas por región", Tipo=TipoGrafico.Barras, Dimension="Region", Medida="SumaImporte", PermiteDrilldown=true },
        new() { Id="w2", Titulo="Evolución mensual", Tipo=TipoGrafico.Timeline, Dimension="Mes", Medida="SumaImporte" },
        new() { Id="w3", Titulo="Top productos", Tipo=TipoGrafico.Pie, Dimension="Producto", Medida="Cantidad", PermiteDrilldown=true },
    };
}
```

Es básicamente una lista declarativa en C#, no hace falta un motor de metadatos complejo para empezar.

### 6. Endpoint htmx: pintar todo con el filtro aplicado

csharp

```csharp
app.MapPost("/dashboard/ventas/refrescar", (FiltroDashboard filtro, VentasQueries q, DashboardVentas dash) =>
{
    var baseQuery = q.Base().AplicarFiltro(filtro);
    var vm = dash.Widgets.Select(w => new WidgetResultado(w, Calcular(baseQuery, w)));
    return Results.Extensions.RazorPartial("_DashboardWidgets", vm);
});
```

En la vista principal:

html

```html
<form hx-post="/dashboard/ventas/refrescar" hx-target="#widgets" hx-trigger="change">
    <input type="date" name="Desde">
    <input type="date" name="Hasta">
    <select name="RegionId">...</select>
</form>

<div id="widgets">
    @await Html.PartialAsync("_DashboardWidgets", Model)
</div>
```

Cada cambio de filtro dispara `hx-post`, htmx reemplaza `#widgets` completo, y ahí van todos los gráficos ya recalculados con el nuevo filtro. Simple y centralizado.

### 7. Gráficos: Chart.js + re-init tras swap

htmx solo mueve HTML, no ejecuta JS de gráficos automáticamente. Patrón típico: cada widget es un `<canvas>` con los datos serializados en un atributo `data-*`, y escuchas el evento `htmx:afterSwap` para reinicializar:

html

```html
<canvas class="chart-widget" data-tipo="Barras" data-json='@Html.Raw(w.DatosJson)'></canvas>
```

js

```js
document.body.addEventListener('htmx:afterSwap', () => {
    document.querySelectorAll('.chart-widget').forEach(c => {
        new Chart(c, buildConfig(c.dataset.tipo, JSON.parse(c.dataset.json)));
    });
});
```

### 8. Drilldown e ir al detalle

Dos niveles:

- **Drilldown a otra dimensión** (ej. clic en "Región Norte" → desglose por producto dentro de esa región): el clic en la barra dispara `hx-get` a un endpoint tipo `/dashboard/ventas/drilldown?dimension=Producto&regionId=3` que reusa la misma `VentasQueries.Base()` + filtro + el nuevo `Where`, devolviendo un widget hijo.
- **Ir a datos concretos** (la tabla de facts detalle): un enlace `hx-get="/dashboard/ventas/detalle?regionId=3&mes=2026-07"` que devuelve una tabla paginada con las filas de `FactVenta` que cumplen esos criterios — sin agregación, el registro crudo.

Ambos casos reutilizan la **misma consulta base + filtro global**, solo añaden condiciones extra. Eso es la clave para que todo sea consistente: nunca escribes una query nueva desde cero, siempre partes de `Base().AplicarFiltro(...)` y le añades el criterio del drilldown.

---

**Resumen de las piezas y dónde vive cada responsabilidad:**

| Capa                              | Responsabilidad                                    |
| --------------------------------- | -------------------------------------------------- |
| Entidades EF                      | Facts + Dimensiones, relaciones                    |
| `*Queries`                        | Consultas base reutilizables (joins)               |
| `FiltroDashboard`                 | Estado de filtro, aplicado a toda consulta         |
| `WidgetDefinicion` / `Dashboard*` | Qué mostrar y cómo (declarativo)                   |
| Endpoint htmx                     | Aplica filtro + calcula widgets + devuelve partial |
| JS (Chart.js)                     | Pinta tras cada swap                               |
| Endpoints drilldown/detalle       | Reusan base+filtro, añaden condición puntual       |

¿Quieres que profundice en alguna parte concreta — por ejemplo cómo resolver dinámicamente la "Dimensión" con expression trees, o cómo estructurar el drilldown para que sea genérico y no tengas que escribir un endpoint por cada combinación?

Lo del drilldown me interesa

Architected reusable drilldown system leveraging hierarchies and state management

Architected reusable drilldown system leveraging hierarchies and state management

Vamos al detalle. El objetivo es que el drilldown sea **genérico**: un solo endpoint que sirva para cualquier widget y cualquier nivel, en vez de un endpoint por combinación.

### 1. Idea central: cadena de dimensiones (jerarquía)

Cada widget no define una sola dimensión, sino una **ruta de drilldown**: una lista ordenada de dimensiones por las que se puede ir bajando.

csharp

```csharp
public class WidgetDefinicion
{
    public string Id { get; set; }
    public string Titulo { get; set; }
    public TipoGrafico Tipo { get; set; }
    public string Medida { get; set; }

    // Antes: Dimension = "Region"
    // Ahora: una jerarquía de niveles
    public List<string> JerarquiaDimensiones { get; set; } = new() { "Region", "Producto", "Cliente" };
}
```

Nivel 0 = `Region`, nivel 1 = `Producto` (dentro de la región elegida), nivel 2 = `Cliente` (dentro de esa región+producto), y tras el último nivel, el "drilldown final" es la tabla de detalle (facts crudos).

### 2. Estado del drilldown: una lista de selecciones acumuladas

En vez de pasar `regionId=3` suelto, modela el camino recorrido como una lista de pares dimensión/valor:

csharp

```csharp
public record SeleccionDrilldown(string Dimension, string Valor);

public class EstadoDrilldown
{
    public string WidgetId { get; set; }
    public List<SeleccionDrilldown> Camino { get; set; } = new();
}
```

Esto viaja en el HTML como inputs ocultos (o como querystring serializado), y cada clic añade un elemento nuevo a `Camino`.

### 3. Resolver dinámicamente la dimensión con expression trees

Aquí es donde conviene un mapa de dimensiones "resolvibles", tanto para agrupar como para filtrar por valor concreto:

csharp

```csharp
public class DimensionResolver
{
    // Cómo agrupar la query por esta dimensión (para el gráfico)
    private static readonly Dictionary<string, Expression<Func<FactVenta, object>>> _agrupadores = new()
    {
        ["Region"]   = f => f.Region.Nombre,
        ["Producto"] = f => f.Producto.Nombre,
        ["Cliente"]  = f => f.Cliente.Nombre,
        ["Mes"]      = f => new DateTime(f.Fecha.Year, f.Fecha.Month, 1),
    };

    // Cómo filtrar la query cuando ya se eligió un valor concreto
    private static readonly Dictionary<string, Func<IQueryable<FactVenta>, string, IQueryable<FactVenta>>> _filtros = new()
    {
        ["Region"]   = (q, v) => q.Where(f => f.Region.Nombre == v),
        ["Producto"] = (q, v) => q.Where(f => f.Producto.Nombre == v),
        ["Cliente"]  = (q, v) => q.Where(f => f.Cliente.Nombre == v),
    };

    public Expression<Func<FactVenta, object>> Agrupador(string dim) => _agrupadores[dim];

    public IQueryable<FactVenta> AplicarSeleccion(IQueryable<FactVenta> q, List<SeleccionDrilldown> camino)
    {
        foreach (var paso in camino)
            q = _filtros[paso.Dimension](q, paso.Valor);
        return q;
    }
}
```

Esto es más mantenible que expression trees "mágicos": cada dimensión declara explícitamente cómo agrupar y cómo filtrar. Añadir una dimensión nueva = añadir dos entradas al diccionario.

### 4. Endpoint único de drilldown

csharp

```csharp
app.MapPost("/dashboard/ventas/drilldown", (
    EstadoDrilldown estado,
    FiltroDashboard filtroGlobal,
    VentasQueries q,
    DashboardVentas dash,
    DimensionResolver resolver) =>
{
    var widget = dash.Widgets.First(w => w.Id == estado.WidgetId);
    int nivelActual = estado.Camino.Count;

    // 1. Base + filtro global de pantalla
    var query = q.Base().AplicarFiltro(filtroGlobal);

    // 2. Aplicar todo lo ya seleccionado en el camino de drilldown
    query = resolver.AplicarSeleccion(query, estado.Camino);

    // 3. ¿Quedan niveles de jerarquía o ya llegamos al final?
    if (nivelActual < widget.JerarquiaDimensiones.Count)
    {
        var dimSiguiente = widget.JerarquiaDimensiones[nivelActual];
        var datos = query
            .GroupBy(resolver.Agrupador(dimSiguiente))
            .Select(g => new { Etiqueta = g.Key, Valor = g.Sum(x => x.Importe) })
            .ToList();

        return Results.Extensions.RazorPartial("_WidgetDrilldown", new WidgetDrilldownVM(widget, dimSiguiente, estado.Camino, datos));
    }
    else
    {
        // Se acabó la jerarquía → tabla de detalle (facts crudos), paginada
        var detalle = query.OrderByDescending(f => f.Fecha).Take(100).ToList();
        return Results.Extensions.RazorPartial("_TablaDetalle", detalle);
    }
});
```

Un único endpoint sirve para **cualquier widget** y **cualquier nivel**, porque toda la lógica de "qué dimensión toca ahora" sale de `widget.JerarquiaDimensiones` + `estado.Camino.Count`.

### 5. HTML/htmx: cada barra dispara el siguiente nivel

html

```html
@foreach (var item in Model.Datos)
{
    <button
        hx-post="/dashboard/ventas/drilldown"
        hx-vals='{"WidgetId": "@Model.Widget.Id",
                  "Camino": @Html.Raw(Json.Serialize(Model.CaminoActual.Append(new { Dimension = Model.DimensionActual, Valor = item.Etiqueta })))}'
        hx-target="closest .widget-container"
        hx-swap="outerHTML">
        @item.Etiqueta — @item.Valor.ToString("C")
    </button>
}
```

Cada clic manda el camino acumulado + el paso nuevo. El servidor responde con el widget "un nivel más adentro", que se sustituye a sí mismo (`hx-swap="outerHTML"` sobre su propio contenedor).

### 6. Breadcrumb y volver atrás

Como el estado es una lista, el breadcrumb sale gratis: pintas `Region: Norte > Producto: Zapatillas` con cada segmento siendo un botón que dispara el mismo endpoint pero con `Camino` truncado hasta ese punto (es decir, "volver" es solo reenviar una lista más corta).

html

```html
<nav class="breadcrumb">
    <button hx-post="/dashboard/ventas/drilldown" hx-vals='{"WidgetId":"w1","Camino":[]}'>Inicio</button>
    @foreach (var (paso, i) in Model.CaminoActual.Select((p, i) => (p, i)))
    {
        <button hx-post="/dashboard/ventas/drilldown"
                hx-vals='{"WidgetId":"w1","Camino": @Html.Raw(Json.Serialize(Model.CaminoActual.Take(i+1)))}'>
            @paso.Dimension: @paso.Valor
        </button>
    }
</nav>
```

### 7. Ir a "datos concretos" desde cualquier nivel (no solo al final)

Si quieres permitir saltar a la tabla de detalle sin agotar la jerarquía, añade un botón "Ver registros" en cada nivel intermedio que pegue al mismo endpoint pero con un flag `forzarDetalle: true`, ignorando cuántos niveles quedan.

### Resumen de por qué esto escala bien

| Elemento                             | Ventaja                                                                      |
| ------------------------------------ | ---------------------------------------------------------------------------- |
| `JerarquiaDimensiones` en el widget  | Defines el orden de drilldown declarativamente, una vez                      |
| `Camino` como lista acumulada        | Breadcrumb, "volver", y estado del drilldown salen gratis                    |
| `DimensionResolver` con diccionarios | Añadir una dimensión nueva no toca el endpoint                               |
| Un solo endpoint                     | No escribes un handler por cada combinación de nivel/widget                  |
| Reutiliza `Base().AplicarFiltro()`   | El filtro global de pantalla sigue aplicando en cualquier nivel de drilldown |

¿Quieres que veamos cómo guardar el `Camino` en el propio HTML (para que sobreviva a un refresco de página) o cómo paginar/ordenar la tabla de detalle final con htmx?

Restructuró arquitectura para soportar múltiples cubos con filtros compartidos globales

Restructuró arquitectura para soportar múltiples cubos con filtros compartidos globales

Este es un cambio de enfoque importante: pasas de "una pantalla = un cubo" a "una pantalla = varios cubos con un filtro común". Vale la pena modelarlo bien porque cambia dos cosas: el filtro deja de ser específico de un fact, y el "drilldown" deja de ser un estado por widget — se convierte en **una actualización del filtro de pantalla**, que es compartido.

### 1. El filtro ya no es por cubo — es por concepto de negocio

Antes `FiltroDashboard` tenía campos ligados a `FactVenta` (`RegionId`, `CategoriaId`). Ahora necesitas un filtro que hable de **dimensiones de negocio abstractas**, no de columnas concretas:

csharp

```csharp
public enum DimensionCompartida { Region, Producto, Cliente, Categoria, Mes }

public record SeleccionFiltro(DimensionCompartida Dimension, int Id, string Etiqueta);

public class FiltroPantalla
{
    public DateTime? Desde { get; set; }
    public DateTime? Hasta { get; set; }
    public List<SeleccionFiltro> Selecciones { get; set; } = new(); // Region=3, Categoria=7, etc.
}
```

Nota que ya no es una jerarquía ordenada de drilldown — es un **conjunto de condiciones acumuladas**, porque puede venir de clicar en distintos widgets de distintos cubos, no de bajar niveles dentro de uno solo. Como mucho una `SeleccionFiltro` por dimensión (si ya hay `Region`, un nuevo clic en región reemplaza, no apila).

### 2. Cada cubo declara qué dimensiones compartidas soporta y cómo

Aquí está la pieza clave: no todos los cubos tienen todas las dimensiones. "Altas de clientes" quizá no tiene `Producto`. Cada cubo lo declara explícitamente:

csharp

```csharp
public interface ICuboDefinicion<TFact>
{
    IQueryable<TFact> ConsultaBase();

    // Solo las dimensiones que este cubo soporta, con su expresión de filtro
    IReadOnlyDictionary<DimensionCompartida, Func<IQueryable<TFact>, int, IQueryable<TFact>>> FiltrosSoportados { get; }

    // Solo las dimensiones por las que este cubo puede agrupar (para el chart)
    IReadOnlyDictionary<DimensionCompartida, Expression<Func<TFact, ClaveDimension>>> Agrupadores { get; }

    Expression<Func<TFact, DateTime>> CampoFecha { get; } // para Desde/Hasta, todos los cubos deberían tenerlo
}

public class VentasCubo : ICuboDefinicion<FactVenta>
{
    public IQueryable<FactVenta> ConsultaBase() => _db.Set<FactVenta>().Include(f => f.Region).Include(f => f.Producto);

    public IReadOnlyDictionary<DimensionCompartida, Func<IQueryable<FactVenta>, int, IQueryable<FactVenta>>> FiltrosSoportados => new Dictionary<...>
    {
        [DimensionCompartida.Region]   = (q, id) => q.Where(f => f.RegionId == id),
        [DimensionCompartida.Producto] = (q, id) => q.Where(f => f.ProductoId == id),
        [DimensionCompartida.Categoria]= (q, id) => q.Where(f => f.Producto.CategoriaId == id),
    };

    public Expression<Func<FactVenta, DateTime>> CampoFecha => f => f.Fecha;
    // ...
}

public class AltasClientesCubo : ICuboDefinicion<FactAltaCliente>
{
    public IReadOnlyDictionary<DimensionCompartida, Func<IQueryable<FactAltaCliente>, int, IQueryable<FactAltaCliente>>> FiltrosSoportados => new Dictionary<...>
    {
        [DimensionCompartida.Region] = (q, id) => q.Where(f => f.RegionId == id),
        // Nota: no incluye Producto ni Categoria, porque no aplica a este cubo
    };
    // ...
}
```

### 3. Aplicar el filtro de pantalla a un cubo — ignorando lo que no aplica

csharp

```csharp
public static class FiltroExtensions
{
    public static IQueryable<TFact> AplicarFiltroPantalla<TFact>(
        this IQueryable<TFact> query,
        ICuboDefinicion<TFact> cubo,
        FiltroPantalla filtro)
    {
        if (filtro.Desde != null)
            query = query.Where(FiltroExpr(cubo.CampoFecha, f => f >= filtro.Desde));
        if (filtro.Hasta != null)
            query = query.Where(FiltroExpr(cubo.CampoFecha, f => f <= filtro.Hasta));

        foreach (var seleccion in filtro.Selecciones)
        {
            if (cubo.FiltrosSoportados.TryGetValue(seleccion.Dimension, out var aplicar))
                query = aplicar(query, seleccion.Id);
            // si el cubo no soporta esa dimensión, se ignora silenciosamente
        }

        return query;
    }
}
```

Esta es la respuesta directa a tu planteamiento: **si filtras por región a nivel de pantalla, se aplica a todos los cubos que tengan región** — y los que no la tengan simplemente no se ven afectados por ese filtro (ni deberían romperse).

### 4. El widget ahora referencia su propio cubo

csharp

```csharp
public class WidgetDefinicion<TFact>
{
    public string Id { get; set; }
    public string Titulo { get; set; }
    public TipoGrafico Tipo { get; set; }
    public ICuboDefinicion<TFact> Cubo { get; set; }
    public string Medida { get; set; }
    public DimensionCompartida? Desglose { get; set; } // por qué dimensión se agrupa el chart (puede ser null = solo serie temporal)
}
```

csharp

```csharp
var widgets = new List<IWidgetDefinicion>
{
    new WidgetDefinicion<FactVenta> { Id="w1", Titulo="Ventas por mes", Cubo=ventasCubo, Medida="SumaImporte", Desglose=DimensionCompartida.Mes },
    new WidgetDefinicion<FactAltaCliente> { Id="w2", Titulo="Altas de clientes por mes", Cubo=altasCubo, Medida="NumAltas", Desglose=DimensionCompartida.Mes },
    new WidgetDefinicion<FactVenta> { Id="w3", Titulo="Ventas por categoría", Cubo=ventasCubo, Medida="SumaImporte", Desglose=DimensionCompartida.Categoria },
};
```

`IWidgetDefinicion` es una interfaz no genérica mínima (Id, Titulo, Tipo) para poder meter widgets de distinto `TFact` en una misma lista de pantalla.

### 5. Modelo de pantalla: ya sin `TipoCubo` único

csharp

```csharp
public class PantallaDashboard
{
    public string Id { get; set; }
    public string Titulo { get; set; }
    public List<IWidgetDefinicion> Widgets { get; set; }
    public List<CampoFiltro> FiltrosDisponibles { get; set; } // qué controles de filtro se muestran arriba
}
```

`FiltrosDisponibles` es la lista de controles visibles (`Región`, `Categoría`, fechas). Como es a nivel de pantalla, se muestra uno solo aunque varios cubos lo usen — el usuario no ve "un filtro de región por cada gráfico", ve uno arriba que afecta a todo.

### 6. Clic en un widget → actualiza el filtro de pantalla, no un estado local

Este es el cambio de comportamiento clave frente al modelo anterior. Antes el clic mandaba `Camino` (estado propio del widget). Ahora el clic simplemente **añade/reemplaza una `SeleccionFiltro` en el `FiltroPantalla`** y dispara el recálculo de **todos** los widgets:

html

```html
@* Widget w3: Ventas por categoría, cada barra es una categoría *@
@foreach (var item in Model.Datos)
{
    <button
        hx-get="/dashboard/refrescar?@Model.FiltroActualQueryString&Selecciones=Categoria:@item.Id:@Uri.EscapeDataString(item.Etiqueta)"
        hx-target="#dashboard-completo"
        hx-swap="innerHTML"
        hx-push-url="true">
        @item.Etiqueta — @item.Valor.ToString("C")
    </button>
}
```

csharp

```csharp
app.MapGet("/dashboard/refrescar", (
    [AsParameters] FiltroPantallaParams parametros,
    PantallaDashboard pantalla) =>
{
    var filtro = parametros.AFiltroPantalla(); // parsea querystring a FiltroPantalla

    var resultados = pantalla.Widgets
        .Select(w => w.Calcular(filtro)) // cada widget aplica el filtro a SU cubo
        .ToList();

    return Results.Extensions.RazorPartial("_DashboardCompleto", new DashboardVM(pantalla, filtro, resultados));
});
```

Fíjate que el `hx-target` ya no es `closest .widget-container` (un solo widget) sino **todo el contenedor de la pantalla** (`#dashboard-completo`) — porque un clic en un gráfico ahora repinta los demás también.

### 7. Breadcrumb de filtros activos (aplica a toda la pantalla, no a un widget)

html

```html
<div class="filtros-activos">
    @foreach (var s in Model.Filtro.Selecciones)
    {
        <span class="chip">
            @s.Dimension: @s.Etiqueta
            <button hx-get="/dashboard/refrescar?@Model.QuitarSeleccion(s.Dimension)"
                    hx-target="#dashboard-completo" hx-push-url="true">✕</button>
        </span>
    }
</div>
```

Cada "chip" quitable regenera la URL sin esa selección — vuelve a recalcular todos los widgets sin ese filtro.

### 8. Diferencia clave vs. el modelo de drilldown por widget que vimos antes

|                                                                 | Drilldown por widget (antes)                                            | Filtro de pantalla (ahora)                                                           |
| --------------------------------------------------------------- | ----------------------------------------------------------------------- | ------------------------------------------------------------------------------------ |
| Estado                                                          | `Camino` propio de un widget, jerárquico y ordenado                     | `FiltroPantalla.Selecciones`, un set de condiciones, sin orden obligatorio           |
| Alcance del clic                                                | Solo afecta al widget en el que clicaste                                | Afecta a **todos** los widgets de la pantalla                                        |
| Aplica a                                                        | Un solo cubo                                                            | Varios cubos (cada uno filtra lo que le aplica)                                      |
| ¿Sigue teniendo sentido la jerarquía `Region→Producto→Cliente`? | Sí, para "seguir bajando" dentro del mismo widget si quieres mantenerlo | Opcional: puedes seguir permitiendo eso *dentro* de un widget concreto (ver punto 9) |

### 9. ¿Pierdes el drilldown jerárquico? No, se puede combinar

Los dos modelos no son excluyentes. Puedes seguir teniendo, **dentro de un widget concreto**, un drilldown de "ver más detalle sin tocar el filtro global" (por ejemplo, expandir una barra a su desglose interno sin afectar a los demás gráficos), y **por separado**, un clic explícito de "usar esto como filtro de pantalla" (ej. un botón "Filtrar por esto 🔍" al lado de cada barra). Son dos acciones distintas con distinta semántica — vale la pena diferenciarlas en la UI para que el usuario sepa cuál va a afectar solo a ese gráfico y cuál afecta a toda la pantalla.

### Resumen del modelo final

```
PantallaDashboard
 ├─ FiltrosDisponibles: List<CampoFiltro>        (controles visibles arriba)
 └─ Widgets: List<IWidgetDefinicion>
     └─ WidgetDefinicion<TFact>
         ├─ Cubo: ICuboDefinicion<TFact>          (su propia consulta base)
         ├─ Medida
         └─ Desglose: DimensionCompartida?

FiltroPantalla                                     (estado compartido, vive en la URL)
 ├─ Desde / Hasta
 └─ Selecciones: List<SeleccionFiltro>             (Dimension, Id, Etiqueta)

Al refrescar:
  foreach widget en pantalla.Widgets:
      widget.Cubo.ConsultaBase()
            .AplicarFiltroPantalla(widget.Cubo, filtroPantalla)   // ignora lo que no aplica
            .GroupBy(widget.Desglose)
            .Calcular(widget.Medida)
```

¿Quieres que veamos cómo tipar `IWidgetDefinicion.Calcular(filtro)` para que funcione de forma genérica sin `dynamic` ni reflexión pesada, dado que cada widget tiene un `TFact` distinto?

Que ventajas y que inconvenientes tiene este planteamiento

Ponderó ventajas e inconvenientes del enfoque multicubo

Ponderó ventajas e inconvenientes del enfoque multicubo

Buen momento para hacer balance antes de seguir construyendo encima. Vamos con ventajas e inconvenientes reales, no solo los obvios.

### Ventajas

**1. Refleja el modelo mental del usuario de negocio**  
Un director que mira "ventas, altas de clientes y volumen de negocio por región" quiere filtrar una vez y ver todo coherente. Este modelo lo hace posible sin trucos — es la arquitectura correcta para ese caso de uso, no un parche.

**2. Cada cubo permanece desacoplado**  
`VentasCubo` no sabe que existe `AltasClientesCubo`. Puedes añadir un tercer cubo (`FactDevoluciones`) sin tocar los otros dos, siempre que declare qué dimensiones compartidas soporta. Bajo acoplamiento real.

**3. El filtro "ignora silenciosamente" lo que no aplica**  
Es una decisión de diseño acertada: si un cubo no tiene `Producto`, simplemente no se ve afectado por ese filtro, en vez de fallar o necesitar lógica condicional en cada sitio.

**4. Reutilización fuerte de infraestructura**  
El motor de "aplicar filtro → agrupar → pintar" es el mismo para cualquier combinación de cubos. Añadir un widget nuevo es declarativo (una entrada más en la lista), no código nuevo.

**5. Estado en URL, compartible, con historial de navegador funcionando**  
Ya lo resolvimos antes y se mantiene: enlaces compartibles, F5 no rompe nada, "atrás" funciona nativamente con htmx.

### Inconvenientes y riesgos

**1. Rendimiento: N cubos = N queries por cada interacción**  
Cada clic dispara un recálculo de **todos** los widgets, cada uno con su propia query a la base de datos. Con 3 gráficos no pasa nada; con 8-10 widgets en pantalla, cada filtro hace 8-10 roundtrips a la BD en paralelo (o secuenciales, según cómo lo implementes). Hay que vigilar:

- Ejecutar las queries en paralelo (`Task.WhenAll`), no secuencial.
- Cachear agregados si el volumen de datos es grande.
- Considerar que si un cubo es muy pesado (millones de filas), un filtro "inocente" en otro widget puede disparar una query cara de fondo aunque el usuario no esté mirando ese gráfico.

**2. Ambigüedad semántica entre dimensiones con el mismo nombre pero distinto significado**  
`DimensionCompartida.Region` asume que "región" significa lo mismo en ventas, altas de clientes y devoluciones. En la práctica esto casi nunca es 100% cierto: puede que "región del cliente" y "región de la venta" no coincidan (un cliente de Madrid compra un producto enviado a Barcelona). Si no se documenta bien qué significa cada dimensión compartida en cada cubo, terminas con **filtros que parecen coherentes pero comparan cosas distintas** — un riesgo silencioso y difícil de detectar.

**3. Pérdida de "contexto" al ver un gráfico ya muy filtrado**  
Si aplicas 4-5 selecciones acumuladas (región + categoría + cliente + mes), algún cubo puede quedarse sin datos (0 resultados) mientras otros siguen mostrando algo. Hay que diseñar bien el estado vacío por widget, porque no todos "fallan" al mismo tiempo ni de la misma forma.

**4. El filtro deja de tener tipos fuertes**  
`FiltroPantalla.Selecciones` como lista de `(Dimension, Id, Etiqueta)` es flexible pero pierde el tipado fuerte que teníamos antes (`RegionId int?` explícito). Errores como "pasar un Id de Producto donde se espera un Id de Región" ya no los detecta el compilador — se detectan en tiempo de ejecución, o no se detectan y simplemente no filtran nada (porque el cubo no reconoce esa combinación). Es un trade-off consciente: flexibilidad a cambio de menos garantías en compilación.

**5. La UI se complica: hay que comunicar bien qué es "filtro global" vs "detalle local"**  
Ya lo mencionamos: si permites drilldown local (solo afecta a un widget) *y* filtro de pantalla (afecta a todos), el usuario tiene que entender la diferencia. Si no se diseña con cuidado (iconos, colores, textos claros tipo "🔍 Usar como filtro" vs "Ver detalle"), es fácil que el usuario haga clic pensando que solo afecta a un gráfico y se sorprenda cuando cambian todos.

**6. Testing y depuración algo más difíciles**  
Con un cubo por pantalla, testear "el filtro X produce el resultado Y" es directo. Con N cubos reaccionando al mismo filtro, tienes que testear la matriz completa: cada cubo con cada combinación relevante de selecciones, incluyendo los casos donde una dimensión no aplica. Más superficie de pruebas.

**7. Granularidad temporal distinta entre cubos**  
"Ventas por mes" y "Altas de clientes por mes" son fáciles de alinear. Pero si un cubo agrega por semana y otro por mes de forma nativa (por cómo se genera el dato), forzar ambos a "Mes" como dimensión compartida puede requerir normalización extra en cada `Agrupador`, no siempre trivial.

### En resumen

|                      |                                                                                                                                                                            |
| -------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Cuándo compensa**  | Pantallas ejecutivas/resumen con pocos cubos (3-6), volumen de datos moderado, dimensiones de negocio genuinamente compartidas (región, fecha)                             |
| **Cuándo cuidado**   | Muchos widgets pesados a la vez, dimensiones que *parecen* iguales pero no lo son semánticamente, necesidad de tipado fuerte y validación estricta                         |
| **Mitigación clave** | Documentar explícitamente qué significa cada `DimensionCompartida` por cubo, paralelizar las queries del refresh, diferenciar visualmente drilldown local vs filtro global |

Es una arquitectura sólida para el caso que describes (pantalla ejecutiva multi-KPI), el precio es más disciplina en la definición semántica de las dimensiones compartidas y algo de cuidado en rendimiento cuando crezca el número de widgets. No es un mal planteamiento — es el correcto para ese objetivo, simplemente hay que entrar con los ojos abiertos en esos puntos.
