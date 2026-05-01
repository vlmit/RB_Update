<?xml version="1.0" encoding="utf-8"?>
<SchemeTable Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="4a5d1567-6e66-4057-83b3-c9eeb3248f42" Name="TaskTextTemplate" Group="Custom" InstanceType="Cards" ContentType="Entries">
	<Description>Шаблон текста поручения</Description>
	<SchemeComplexColumn IsSystem="true" IsPermanent="true" IsSealed="true" ID="4a5d1567-6e66-0057-2000-09eeb3248f42" Name="ID" Type="Reference(Typified) Not Null" ReferencedTable="1074eadd-21d7-4925-98c8-40d1e5f0ca0e">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="4a5d1567-6e66-0157-4000-09eeb3248f42" Name="ID" Type="Guid Not Null" ReferencedColumn="9a58123b-b2e9-4137-9c6c-5dab0ec02747" />
	</SchemeComplexColumn>
	<SchemePhysicalColumn ID="261dc8e1-46f9-4863-9adc-6f42ed8e1c94" Name="Name" Type="String(Max) Null" />
	<SchemePrimaryKey IsSystem="true" IsPermanent="true" IsSealed="true" ID="4a5d1567-6e66-0057-5000-09eeb3248f42" Name="pk_TaskTextTemplate" IsClustered="true">
		<SchemeIndexedColumn Column="4a5d1567-6e66-0157-4000-09eeb3248f42" />
	</SchemePrimaryKey>
</SchemeTable>