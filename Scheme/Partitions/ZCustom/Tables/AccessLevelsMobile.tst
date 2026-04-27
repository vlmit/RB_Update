<?xml version="1.0" encoding="utf-8"?>
<SchemeTable Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="b747068f-28d2-44a2-bfba-5db0a2f5743f" Name="AccessLevelsMobile" Group="Custom">
	<Description>Уровни доступа для мобильного клиента</Description>
	<SchemePhysicalColumn ID="d63e19aa-30b9-40d2-aae2-f899e908bcdc" Name="Name" Type="String(128) Not Null" />
	<SchemePhysicalColumn ID="efd48aea-d835-49ca-8e48-7856609fe75d" Name="IsConfidential" Type="Boolean Null">
		<SchemeDefaultConstraint IsPermanent="true" ID="d24523a8-bde7-419b-acfc-c5b0355821f3" Name="df_AccessLevelsMobile_IsConfidential" Value="false" />
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="3a9d9098-b039-44ab-814a-687921d507fc" Name="Weight" Type="Int64 Null" />
	<SchemePhysicalColumn ID="1c62e95e-4707-49ed-9557-49326fb92452" Name="ID" Type="Int32 Not Null" />
	<SchemePrimaryKey ID="89a1f0f6-966c-43c9-910b-e8f82c3ccf16" Name="pk_AccessLevelsMobile">
		<SchemeIndexedColumn Column="1c62e95e-4707-49ed-9557-49326fb92452" />
	</SchemePrimaryKey>
	<SchemeIndex ID="12eb4735-7d74-428a-b52c-b3d3ecec0540" Name="ndx_AccessLevelsMobile_ID" IsUnique="true">
		<SchemeIndexedColumn Column="1c62e95e-4707-49ed-9557-49326fb92452" />
	</SchemeIndex>
	<SchemeRecord>
		<Name ID="d63e19aa-30b9-40d2-aae2-f899e908bcdc">Общий</Name>
		<IsConfidential ID="efd48aea-d835-49ca-8e48-7856609fe75d">false</IsConfidential>
		<Weight ID="3a9d9098-b039-44ab-814a-687921d507fc">1</Weight>
		<ID ID="1c62e95e-4707-49ed-9557-49326fb92452">1</ID>
	</SchemeRecord>
</SchemeTable>