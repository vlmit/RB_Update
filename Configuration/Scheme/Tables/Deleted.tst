<?xml version="1.0" encoding="utf-8"?>
<SchemeTable ID="a49102cc-6bb4-425b-95ad-75ff0b3edf0d" Name="Deleted" Group="System" InstanceType="Cards" ContentType="Entries">
	<Description>Информация об удалённой карточке.</Description>
	<SchemeComplexColumn IsSystem="true" IsPermanent="true" IsSealed="true" ID="a49102cc-6bb4-005b-2000-05ff0b3edf0d" Name="ID" Type="Reference(Typified) Not Null" ReferencedTable="1074eadd-21d7-4925-98c8-40d1e5f0ca0e">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="a49102cc-6bb4-015b-4000-05ff0b3edf0d" Name="ID" Type="Guid Not Null" ReferencedColumn="9a58123b-b2e9-4137-9c6c-5dab0ec02747" />
	</SchemeComplexColumn>
	<SchemePhysicalColumn ID="8ec16f22-aef1-4f39-8f88-68589e9c017a" Name="Digest" Type="String(128) Null">
		<Description>Краткое описание карточки в момент удаления.</Description>
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="5509ca1e-1527-48a3-9bdc-aff6f47c177d" Name="Card" Type="BinaryJson Not Null">
		<Description>Удалённая карточка в сериализованном виде.</Description>
		<SchemeDefaultConstraint IsPermanent="true" ID="3627013e-57d1-4ee9-b32f-6c8641db6093" Name="df_Deleted_Card" Value="{}" />
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="6a80b8a6-bd38-4b81-a747-8c39a9e68dd1" Name="CardID" Type="Guid Not Null">
		<Description>Идентификатор удалённой карточки.</Description>
	</SchemePhysicalColumn>
	<SchemeComplexColumn ID="3bea0d27-bfa9-475c-b771-c4fcb69be8eb" Name="Type" Type="Reference(Typified) Not Null" ReferencedTable="b0538ece-8468-4d0b-8b4e-5a1d43e024db">
		<Description>Тип удалённой карточки.</Description>
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="3bea0d27-bfa9-005c-4000-04fcb69be8eb" Name="TypeID" Type="Guid Not Null" ReferencedColumn="a628a864-c858-4200-a6b7-da78c8e6e1f4">
			<Description>ID of a type.</Description>
		</SchemeReferencingColumn>
		<SchemeReferencingColumn ID="55b69fb9-72b3-4829-8298-076258924c41" Name="TypeCaption" Type="String(128) Not Null" ReferencedColumn="0a02451e-2e06-4001-9138-b4805e641afa">
			<Description>Caption of a type.</Description>
		</SchemeReferencingColumn>
	</SchemeComplexColumn>
	<SchemePrimaryKey IsSystem="true" IsPermanent="true" IsSealed="true" ID="a49102cc-6bb4-005b-5000-05ff0b3edf0d" Name="pk_Deleted" IsClustered="true">
		<SchemeIndexedColumn Column="a49102cc-6bb4-015b-4000-05ff0b3edf0d" />
	</SchemePrimaryKey>
	<SchemeIndex ID="ec3e010f-d04b-4f9d-9f8e-d906841c0b0c" Name="ndx_Deleted_CardID">
		<SchemeIndexedColumn Column="6a80b8a6-bd38-4b81-a747-8c39a9e68dd1" />
	</SchemeIndex>
</SchemeTable>