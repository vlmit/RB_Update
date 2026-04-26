<?xml version="1.0" encoding="utf-8"?>
<SchemeTable ID="7a11fc2e-25a3-47c2-981e-2ae3ea1fa705" Name="PlaceholderCompilationCache" Group="System">
	<Description>Кеш компиляции скриптов текстов с плейсхолдерами</Description>
	<SchemePhysicalColumn ID="033f675c-ac87-4cb1-b071-889d136c7c40" Name="ID" Type="Guid Not Null">
		<Description>Идентификатор объекта, к которому относится результат компиляции</Description>
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="67983e2f-752c-4345-986c-e6e57b5d3677" Name="CompilationResult" Type="Binary(Max) Not Null">
		<Description>Результат компиляции</Description>
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="dc968917-22be-40c5-96f6-8cc0e1625448" Name="Created" Type="DateTime2 Not Null">
		<Description>Дата последнего изменения кеша</Description>
	</SchemePhysicalColumn>
	<SchemePrimaryKey ID="49bf76be-804f-4a4c-bf8b-2fa2ac9afc12" Name="pk_PlaceholderCompilationCache">
		<SchemeIndexedColumn Column="033f675c-ac87-4cb1-b071-889d136c7c40" />
	</SchemePrimaryKey>
</SchemeTable>