<?xml version="1.0" encoding="utf-8"?>
<SchemeTable Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="7f7ada04-46da-4088-a52d-0402cf90703c" Name="RegTasksDataRequests" Group="Custom" InstanceType="Cards" ContentType="Entries">
	<SchemeComplexColumn IsSystem="true" IsPermanent="true" IsSealed="true" ID="7f7ada04-46da-0088-2000-0402cf90703c" Name="ID" Type="Reference(Typified) Not Null" ReferencedTable="1074eadd-21d7-4925-98c8-40d1e5f0ca0e">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="7f7ada04-46da-0188-4000-0402cf90703c" Name="ID" Type="Guid Not Null" ReferencedColumn="9a58123b-b2e9-4137-9c6c-5dab0ec02747" />
	</SchemeComplexColumn>
	<SchemePhysicalColumn ID="a1a5637b-4904-4542-a565-d9888527d283" Name="TaskType" Type="String(Max) Null" />
	<SchemePhysicalColumn ID="e5933d0e-6fe0-4f4b-a0cc-fbfc0532c418" Name="Mnemonic" Type="String(Max) Null" />
	<SchemePhysicalColumn ID="20daa611-13d5-43e0-bb1d-5f349546e6b8" Name="Status" Type="Int16 Null" />
	<SchemePhysicalColumn ID="368f2a30-6026-4f5c-b9a6-2322f0640a83" Name="UpdateAt" Type="DateTime Null" />
	<SchemePhysicalColumn ID="6aad0196-e780-4cb9-9612-869bf2491f00" Name="Message" Type="String(Max) Null" />
	<SchemePhysicalColumn ID="c55b3d97-6e95-4f34-ba1a-ce52912f90f9" Name="RequestID" Type="String(Max) Null" />
	<SchemePhysicalColumn ID="6c068cfa-cc15-4d1f-b146-506933e3acb2" Name="PeriodDateStart" Type="DateTime Null" />
	<SchemePhysicalColumn ID="001edb44-8143-4b23-a474-673dfdef4b3b" Name="PeriodDateEnd" Type="DateTime Null" />
	<SchemePhysicalColumn ID="4e785339-b83e-43ae-b60b-2293c0d5cc63" Name="CsvContent" Type="String(Max) Null" />
	<SchemePrimaryKey IsSystem="true" IsPermanent="true" IsSealed="true" ID="7f7ada04-46da-0088-5000-0402cf90703c" Name="pk_RegTasksDataRequests" IsClustered="true">
		<SchemeIndexedColumn Column="7f7ada04-46da-0188-4000-0402cf90703c" />
	</SchemePrimaryKey>
</SchemeTable>