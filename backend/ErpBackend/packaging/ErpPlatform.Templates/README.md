# ErpPlatform.Templates (scaffold)

`dotnet new` template pack for the ERP Platform backend. **Scaffold base only** — the actual
`erp-module` template (controller + DTOs + validator + repository interface, generated from the same
`<Module>.module.json` manifest) is implemented in **Slice 2 (backend generator)**.

## Planned usage (future)
```bash
dotnet new install ErpPlatform.Templates
dotnet new erp-module --name Customer --manifest Customer.module.json
```

## Layout
```
ErpPlatform.Templates/
├─ ErpPlatform.Templates.csproj   # template pack project (IsPackable=false until templates exist)
└─ templates/                     # template content goes here (each with a .template.config/template.json)
   └─ erp-module/                 # (to be added in Slice 2)
```
Once `templates/erp-module` exists, set `IsPackable=true` and run `dotnet pack` to produce
`ErpPlatform.Templates.1.0.0-rc.0.nupkg`. Not added to the solution to keep current builds untouched.
