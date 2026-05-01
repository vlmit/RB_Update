<?xml version="1.0" encoding="utf-8"?>
<SchemeTable Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="1f340acf-4fd7-47c0-bb50-9de52773d148" Name="SendFiles" Group="Common" InstanceType="Cards" ContentType="Collections">
	<SchemeComplexColumn IsSystem="true" IsPermanent="true" IsSealed="true" ID="1f340acf-4fd7-00c0-2000-0de52773d148" Name="ID" Type="Reference(Typified) Not Null" ReferencedTable="1074eadd-21d7-4925-98c8-40d1e5f0ca0e">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="1f340acf-4fd7-01c0-4000-0de52773d148" Name="ID" Type="Guid Not Null" ReferencedColumn="9a58123b-b2e9-4137-9c6c-5dab0ec02747" />
	</SchemeComplexColumn>
	<SchemePhysicalColumn IsSystem="true" IsPermanent="true" IsSealed="true" ID="1f340acf-4fd7-00c0-3100-0de52773d148" Name="RowID" Type="Guid Not Null" />
	<SchemePhysicalColumn ID="5ed9def7-699c-4db5-9d23-4818499e349b" Name="VersionID" Type="Guid Null" />
	<SchemePhysicalColumn ID="715baf00-f88e-46d4-8dd5-60a8122b00eb" Name="MedoSend" Type="Boolean Not Null">
		<SchemeDefaultConstraint IsPermanent="true" ID="aa504d7b-a4a9-42b8-a758-9574dbb7354e" Name="df_SendFiles_MedoSend" Value="false" />
	</SchemePhysicalColumn>
	<SchemeComplexColumn ID="927255c7-67e0-45ee-9503-1c95745c5569" Name="Files" Type="Reference(Typified) Null" ReferencedTable="dd716146-b177-4920-bc90-b1196b16347c">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="927255c7-67e0-00ee-4000-0c95745c5569" Name="FilesID" Type="Guid Null" ReferencedColumn="dd716146-b177-0020-3100-01196b16347c" />
		<SchemeReferencingColumn ID="ed9189d6-8e16-40d7-9735-80405184ee7c" Name="FilesName" Type="String(256) Null" ReferencedColumn="5fa1d976-21b8-4df5-b52e-f7beadf93e9d" />
	</SchemeComplexColumn>
	<SchemePrimaryKey IsSystem="true" IsPermanent="true" IsSealed="true" ID="1f340acf-4fd7-00c0-5000-0de52773d148" Name="pk_SendFiles">
		<SchemeIndexedColumn Column="1f340acf-4fd7-00c0-3100-0de52773d148" />
	</SchemePrimaryKey>
	<SchemeIndex IsSystem="true" IsPermanent="true" IsSealed="true" ID="1f340acf-4fd7-00c0-7000-0de52773d148" Name="idx_SendFiles_ID" IsClustered="true">
		<SchemeIndexedColumn Column="1f340acf-4fd7-01c0-4000-0de52773d148" />
	</SchemeIndex>
</SchemeTable>