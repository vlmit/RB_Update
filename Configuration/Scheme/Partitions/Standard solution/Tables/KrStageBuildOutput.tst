<?xml version="1.0" encoding="utf-8"?>
<SchemeTable Partition="d1b372f3-7565-4309-9037-5e5a0969d94e" ID="46e5faaa-5c71-4a0d-9cd2-575543fc72b2" Name="KrStageBuildOutput" Group="Kr">
	<Description>Таблица с логами сборки, ID - идентификатор карточки KrStageTemplate или KrCommonMethod. Если ID == Guid.Empty, то сборка общая.</Description>
	<SchemePhysicalColumn ID="0d0ef084-cca6-425c-9a60-e41e08dc216c" Name="ID" Type="Guid Not Null" IsRowGuidColumn="true" />
	<SchemePhysicalColumn ID="f7b88d49-ec6e-48dc-b0d6-d4df043fa7a2" Name="BuildDateTime" Type="DateTime Null">
		<Description>Время сборки</Description>
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="fe516f96-bb43-4651-a575-13f898c33f2d" Name="Output" Type="String(Max) Null">
		<Description>Вывод сборки</Description>
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="582c7a96-50b2-4456-97c1-1f1ae2017507" Name="Assembly" Type="Binary(Max) Null" IsSparse="true" />
	<SchemePhysicalColumn ID="374b7f32-8866-465d-a605-10582fbc6c8c" Name="CompilationResult" Type="Binary(Max) Null" IsSparse="true" />
	<SchemePrimaryKey ID="5928ad66-9e48-435d-a370-adfaabd891e6" Name="pk_KrStageBuildOutput">
		<SchemeIndexedColumn Column="0d0ef084-cca6-425c-9a60-e41e08dc216c" />
	</SchemePrimaryKey>
</SchemeTable>