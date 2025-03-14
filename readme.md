# FiskePrylar

Detta projekt är en webbapp byggd i **ASP.NET Core MVC** och **Entity Framework Core**, där användare kan skapa, hantera och köpa annonser för fiskeprylar. Annonserna innehåller titel, beskrivning, pris, kategori och bilder. Applikationen har användarautentisering och rollbaserad åtkomst för att säkerställa att endast auktoriserade användare kan hantera innehållet. Vi använder en **SQLite-databas** för att lagra information om annonser, användare och kategorier. 

Bildhantering sker genom **SixLabors.ImageSharp**, vilket gör det möjligt att spara och generera miniatyrbilder. Autentiseringen hanteras via **ASP.NET Identity**, vilket ger användarna möjlighet att registrera sig och logga in.

## Scaffolding och Kodgenerering

För att snabba upp utvecklingen används **scaffolding** för att generera kod automatiskt utifrån modeller. På detta sätt skapas controllers och vyer. Med scaffolding genereras även standardiserade **CRUD-funktioner** för annonser och användare. För att hantera inloggning och registrering har identitetssidorna scaffoldats och anpassats efter behov.

## Validering och Dataintegritet

Formulär och inmatningsfält i applikationen har både **server- och klientvalidering** för att förhindra felaktiga inmatningar. Vi använder **Data Annotations** i våra modeller, exempelvis:

- `[Required]` för obligatoriska fält.
- `[StringLength]` för att begränsa textlängd.

## Hantering av Annonser och Bilder

När en annons skapas kan användaren ladda upp bilder som sparas på servern. Vi hanterar bildlagring genom att generera **unika filnamn** och spara dem i en katalog för uppladdade bilder. Om en annons raderas tas även tillhörande bilder bort från systemet.

## Redirect och Flöde

Om en **oinloggad användare** försöker köpa en annons skickas de först till **inloggningssidan** och sedan tillbaka till **annonslistan** efter lyckad inloggning.


## Användare att testa med (Kan också skapa nya)

| Användarnamn     | E-post                  | Lösenord  |
|------------------|-------------------------|-----------|
| fiskare01        | fiskare01@email.com     | Test123!  |
| betesmästaren    | betesproffs@email.com   | Test123!  |
| båtentusiasten   | batguru@email.com       | Test123!  |
| utrustningskung  | gearboss@email.com      | Test123!  |
| allroundfiskare  | allround@email.com      | Test123!  |
