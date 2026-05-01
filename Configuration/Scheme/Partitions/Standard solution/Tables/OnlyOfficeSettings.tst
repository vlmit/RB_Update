<?xml version="1.0" encoding="utf-8"?>
<SchemeTable Partition="d1b372f3-7565-4309-9037-5e5a0969d94e" ID="703ad2d1-6246-4d47-a0ef-814b9466c027" Name="OnlyOfficeSettings" Group="OnlyOffice" InstanceType="Cards" ContentType="Entries">
	<SchemeComplexColumn IsSystem="true" IsPermanent="true" IsSealed="true" ID="703ad2d1-6246-0047-2000-014b9466c027" Name="ID" Type="Reference(Typified) Not Null" ReferencedTable="1074eadd-21d7-4925-98c8-40d1e5f0ca0e">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="703ad2d1-6246-0147-4000-014b9466c027" Name="ID" Type="Guid Not Null" ReferencedColumn="9a58123b-b2e9-4137-9c6c-5dab0ec02747" />
	</SchemeComplexColumn>
	<SchemePhysicalColumn ID="6a1c387c-f572-4c07-8536-4e7f77edea47" Name="ApiScriptUrl" Type="String(Max) Null" />
	<SchemePhysicalColumn ID="30cc4853-115a-42c2-b074-c57f29f46082" Name="ConverterUrl" Type="String(Max) Null" />
	<SchemePhysicalColumn ID="b14a5df0-b7f9-4148-9c5a-b37a83998555" Name="LoadTimeout" Type="Time Not Null">
		<SchemeDefaultConstraint IsPermanent="true" ID="77d83a9f-cfb0-4da2-bb72-a963d724ca33" Name="df_OnlyOfficeSettings_LoadTimeout" Value="PT10M" />
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="b4b34cf7-f698-43b8-a7e6-2f88fe6be50a" Name="PreviewEnabled" Type="Boolean Not Null">
		<SchemeDefaultConstraint IsPermanent="true" ID="81d59e26-951e-467c-8c7b-f81b1c06e57f" Name="df_OnlyOfficeSettings_PreviewEnabled" Value="true" />
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="ca126902-914b-474e-9ed6-7014ca0c715d" Name="ExcludedPreviewFormats" Type="String(Max) Null" />
	<SchemePrimaryKey IsSystem="true" IsPermanent="true" IsSealed="true" ID="703ad2d1-6246-0047-5000-014b9466c027" Name="pk_OnlyOfficeSettings" IsClustered="true">
		<SchemeIndexedColumn Column="703ad2d1-6246-0147-4000-014b9466c027" />
	</SchemePrimaryKey>
</SchemeTable>