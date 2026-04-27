<?xml version="1.0" encoding="utf-8"?>
<SchemeTable Partition="e1b3cb70-f7fc-4b62-b822-e5302d673356" ID="ec645c2d-b316-489c-a300-d0ba41049707" Name="PersonalPartnerListEntry" Group="Junicsoft" InstanceType="Cards" ContentType="Collections">
	<Description>Содержание личного списка контрагентов</Description>
	<SchemeComplexColumn IsSystem="true" IsPermanent="true" IsSealed="true" ID="ec645c2d-b316-009c-2000-00ba41049707" Name="ID" Type="Reference(Typified) Not Null" ReferencedTable="1074eadd-21d7-4925-98c8-40d1e5f0ca0e">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="ec645c2d-b316-019c-4000-00ba41049707" Name="ID" Type="Guid Not Null" ReferencedColumn="9a58123b-b2e9-4137-9c6c-5dab0ec02747" />
	</SchemeComplexColumn>
	<SchemePhysicalColumn IsSystem="true" IsPermanent="true" IsSealed="true" ID="ec645c2d-b316-009c-3100-00ba41049707" Name="RowID" Type="Guid Not Null" />
	<SchemeComplexColumn ID="c8e88380-b351-4a2c-8c7a-6f774e8b72af" Name="Partner" Type="Reference(Typified) Not Null" ReferencedTable="5d47ef13-b6f4-47ef-9815-3b3d0e6d475a">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="c8e88380-b351-002c-4000-0f774e8b72af" Name="PartnerID" Type="Guid Not Null" ReferencedColumn="5d47ef13-b6f4-01ef-4000-0b3d0e6d475a" />
		<SchemeReferencingColumn ID="50110ef8-fde4-449d-963d-72e7958af5cb" Name="PartnerName" Type="String(255) Not Null" ReferencedColumn="f1c960e0-951e-4837-8474-bb61d98f40f0" />
	</SchemeComplexColumn>
	<SchemePhysicalColumn ID="e505cec5-6512-440e-a2fd-c81d6a26416a" Name="SortN" Type="Int16 Null" />
	<SchemePrimaryKey IsSystem="true" IsPermanent="true" IsSealed="true" ID="ec645c2d-b316-009c-5000-00ba41049707" Name="pk_PersonalPartnerListEntry">
		<SchemeIndexedColumn Column="ec645c2d-b316-009c-3100-00ba41049707" />
	</SchemePrimaryKey>
	<SchemeIndex IsSystem="true" IsPermanent="true" IsSealed="true" ID="ec645c2d-b316-009c-7000-00ba41049707" Name="idx_PersonalPartnerListEntry_ID" IsClustered="true">
		<SchemeIndexedColumn Column="ec645c2d-b316-019c-4000-00ba41049707" />
	</SchemeIndex>
</SchemeTable>