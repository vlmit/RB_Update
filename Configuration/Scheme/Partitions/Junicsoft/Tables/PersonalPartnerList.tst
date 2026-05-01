<?xml version="1.0" encoding="utf-8"?>
<SchemeTable Partition="e1b3cb70-f7fc-4b62-b822-e5302d673356" ID="4ba3d202-be30-4f4a-955d-2f8acd697c6f" Name="PersonalPartnerList" Group="Junicsoft" InstanceType="Cards" ContentType="Entries">
	<Description>Личный список контрагентов</Description>
	<SchemeComplexColumn IsSystem="true" IsPermanent="true" IsSealed="true" ID="4ba3d202-be30-004a-2000-0f8acd697c6f" Name="ID" Type="Reference(Typified) Not Null" ReferencedTable="1074eadd-21d7-4925-98c8-40d1e5f0ca0e">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="4ba3d202-be30-014a-4000-0f8acd697c6f" Name="ID" Type="Guid Not Null" ReferencedColumn="9a58123b-b2e9-4137-9c6c-5dab0ec02747" />
	</SchemeComplexColumn>
	<SchemePhysicalColumn ID="7db61bec-e389-4ba4-a226-f3b338da1d68" Name="Title" Type="String(128) Not Null">
		<Description>Название списка</Description>
	</SchemePhysicalColumn>
	<SchemeComplexColumn ID="5e878f2c-72d0-4da1-9dfc-e22397994121" Name="Owner" Type="Reference(Typified) Not Null" ReferencedTable="6c977939-bbfc-456f-a133-f1c2244e3cc3">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="5e878f2c-72d0-00a1-4000-022397994121" Name="OwnerID" Type="Guid Not Null" ReferencedColumn="6c977939-bbfc-016f-4000-01c2244e3cc3" />
		<SchemeReferencingColumn ID="cf04381a-7438-4b89-8349-ee5151ac96b0" Name="OwnerName" Type="String(128) Not Null" ReferencedColumn="1782f76a-4743-4aa4-920c-7edaee860964" />
	</SchemeComplexColumn>
	<SchemePhysicalColumn ID="19a1c871-5285-4046-b47a-74cac988f6cc" Name="Status" Type="Boolean Not Null">
		<Description>Статус</Description>
		<SchemeDefaultConstraint IsPermanent="true" ID="b5bf93be-afd4-4e5b-8e53-d841f6e2f327" Name="df_PersonalPartnerList_Status" Value="false" />
	</SchemePhysicalColumn>
	<SchemePrimaryKey IsSystem="true" IsPermanent="true" IsSealed="true" ID="4ba3d202-be30-004a-5000-0f8acd697c6f" Name="pk_PersonalPartnerList" IsClustered="true">
		<SchemeIndexedColumn Column="4ba3d202-be30-014a-4000-0f8acd697c6f" />
	</SchemePrimaryKey>
	<SchemeIndex ID="c76c5a04-bbea-4f65-bdc5-e1359e2acb4a" Name="ndx_PersonalPartnerList_ID">
		<SchemeIndexedColumn Column="4ba3d202-be30-014a-4000-0f8acd697c6f" />
	</SchemeIndex>
</SchemeTable>