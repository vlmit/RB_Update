<?xml version="1.0" encoding="utf-8"?>
<SchemeTable Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="a7f8fd07-5648-4f06-a0b2-e7c4657b32f3" Name="FamiliarizationRecipients" Group="Custom" InstanceType="Cards" ContentType="Collections">
	<Description>Получатели На ознакомление для карточек  НПА</Description>
	<SchemeComplexColumn IsSystem="true" IsPermanent="true" IsSealed="true" ID="a7f8fd07-5648-0006-2000-07c4657b32f3" Name="ID" Type="Reference(Typified) Not Null" ReferencedTable="1074eadd-21d7-4925-98c8-40d1e5f0ca0e">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="a7f8fd07-5648-0106-4000-07c4657b32f3" Name="ID" Type="Guid Not Null" ReferencedColumn="9a58123b-b2e9-4137-9c6c-5dab0ec02747" />
	</SchemeComplexColumn>
	<SchemePhysicalColumn IsSystem="true" IsPermanent="true" IsSealed="true" ID="a7f8fd07-5648-0006-3100-07c4657b32f3" Name="RowID" Type="Guid Not Null" />
	<SchemeComplexColumn ID="65ab53a4-c7b3-4876-8397-5a3db39219e7" Name="User" Type="Reference(Typified) Not Null" ReferencedTable="6c977939-bbfc-456f-a133-f1c2244e3cc3">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="65ab53a4-c7b3-0076-4000-0a3db39219e7" Name="UserID" Type="Guid Not Null" ReferencedColumn="6c977939-bbfc-016f-4000-01c2244e3cc3" />
		<SchemeReferencingColumn ID="f34e0dfe-f8d3-4d84-bf3d-e70af01e2565" Name="UserName" Type="String(128) Not Null" ReferencedColumn="1782f76a-4743-4aa4-920c-7edaee860964" />
	</SchemeComplexColumn>
	<SchemePrimaryKey IsSystem="true" IsPermanent="true" IsSealed="true" ID="a7f8fd07-5648-0006-5000-07c4657b32f3" Name="pk_FamiliarizationRecipients">
		<SchemeIndexedColumn Column="a7f8fd07-5648-0006-3100-07c4657b32f3" />
	</SchemePrimaryKey>
	<SchemeIndex IsSystem="true" IsPermanent="true" IsSealed="true" ID="a7f8fd07-5648-0006-7000-07c4657b32f3" Name="idx_FamiliarizationRecipients_ID" IsClustered="true">
		<SchemeIndexedColumn Column="a7f8fd07-5648-0106-4000-07c4657b32f3" />
	</SchemeIndex>
</SchemeTable>